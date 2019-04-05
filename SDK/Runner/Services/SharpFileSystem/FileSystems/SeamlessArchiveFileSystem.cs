using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SharpFileSystem.FileSystems
{
    public abstract class SeamlessArchiveFileSystem : IFileSystem
    {
        public static readonly char ArchiveDirectorySeparator = '#';

        private readonly FileSystemUsage _rootUsage;
        private readonly IDictionary<File, FileSystemUsage> _usedArchives = new Dictionary<File, FileSystemUsage>();

        public SeamlessArchiveFileSystem(IFileSystem fileSystem)
        {
            FileSystem = fileSystem;
            _rootUsage = new FileSystemUsage
            {
                Owner = this,
                FileSystem = FileSystem,
                ArchiveFile = null
            };
        }

        public IFileSystem FileSystem { get; }

        public ICollection<FileSystemPath> GetEntities(FileSystemPath path)
        {
            using (var r = Refer(path))
            {
                var fileSystem = r.FileSystem;

                FileSystemPath parentPath;
                if (TryGetArchivePath(path, out parentPath))
                    parentPath = ArchiveFileToDirectory(parentPath);
                else
                    parentPath = FileSystemPath.Root;
                var entities = new LinkedList<FileSystemPath>();
                foreach (var ep in fileSystem.GetEntities(GetRelativePath(path)))
                {
                    var newep = parentPath.AppendPath(ep.ToString().Substring(1));
                    entities.AddLast(newep);
                    if (IsArchiveFile(fileSystem, newep))
                        entities.AddLast(
                            newep.ParentPath.AppendDirectory(newep.EntityName + ArchiveDirectorySeparator));
                }

                return entities;
            }
        }

        public bool Exists(FileSystemPath path)
        {
            using (var r = Refer(path))
            {
                var fileSystem = r.FileSystem;
                return fileSystem.Exists(GetRelativePath(path));
            }
        }

        public Stream OpenFile(FileSystemPath path, FileAccess access)
        {
            var r = Refer(path);
            var s = r.FileSystem.OpenFile(GetRelativePath(path), access);
            return new SafeReferenceStream(s, r);
        }

        public void Dispose()
        {
            foreach (var reference in _usedArchives.Values.SelectMany(usage => usage.References).ToArray())
                UnuseFileSystem(reference);
            FileSystem.Dispose();
        }

        public void UnuseFileSystem(FileSystemReference reference)
        {
            // When root filesystem was used.
            if (reference.Usage.ArchiveFile == null)
                return;

            FileSystemUsage usage;
            if (!_usedArchives.TryGetValue(reference.Usage.ArchiveFile, out usage))
                throw new ArgumentException("The specified reference is not valid.");
            if (!usage.References.Remove(reference))
                throw new ArgumentException("The specified reference does not exist.");
            if (usage.References.Count == 0)
            {
                _usedArchives.Remove(usage.ArchiveFile);

                usage.FileSystem.Dispose();
            }
        }

        protected abstract bool IsArchiveFile(IFileSystem fileSystem, FileSystemPath path);

        private FileSystemPath ArchiveFileToDirectory(FileSystemPath path)
        {
            if (!path.IsFile)
                throw new ArgumentException("The specified path is not a file.");
            return path.ParentPath.AppendDirectory(path.EntityName + ArchiveDirectorySeparator);
        }

        private FileSystemPath GetRelativePath(FileSystemPath path)
        {
            var s = path.ToString();
            var sindex = s.LastIndexOf(ArchiveDirectorySeparator.ToString() + FileSystemPath.DirectorySeparator);
            if (sindex < 0)
                return path;
            return FileSystemPath.Parse(s.Substring(sindex + 1));
        }

        protected bool HasArchive(FileSystemPath path)
        {
            return path.ToString()
                       .LastIndexOf(ArchiveDirectorySeparator.ToString() + FileSystemPath.DirectorySeparator) >= 0;
        }

        protected bool TryGetArchivePath(FileSystemPath path, out FileSystemPath archivePath)
        {
            var p = path.ToString();
            var sindex = p.LastIndexOf(ArchiveDirectorySeparator.ToString() + FileSystemPath.DirectorySeparator);
            if (sindex < 0)
            {
                archivePath = path;
                return false;
            }

            archivePath = FileSystemPath.Parse(p.Substring(0, sindex));
            return true;
        }

        protected FileSystemReference Refer(FileSystemPath path)
        {
            FileSystemPath archivePath;
            if (TryGetArchivePath(path, out archivePath))
                return CreateArchiveReference(archivePath);
            return new FileSystemReference(_rootUsage);
        }

        private FileSystemReference CreateArchiveReference(FileSystemPath archiveFile)
        {
            return CreateReference((File) GetActualLocation(archiveFile));
        }

        private FileSystemReference CreateReference(File file)
        {
            var usage = GetArchiveFs(file);
            var reference = new FileSystemReference(usage);
            usage.References.Add(reference);
            return reference;
        }

        private FileSystemEntity GetActualLocation(FileSystemPath path)
        {
            FileSystemPath archivePath;
            if (!TryGetArchivePath(path, out archivePath))
                return FileSystemEntity.Create(FileSystem, path);
            var archiveFile = (File) GetActualLocation(archivePath);
            var usage = GetArchiveFs(archiveFile);
            return FileSystemEntity.Create(usage.FileSystem, GetRelativePath(path));
        }

        private FileSystemUsage GetArchiveFs(File archiveFile)
        {
            FileSystemUsage usage;
            if (_usedArchives.TryGetValue(archiveFile, out usage)) return usage;

            var archiveFs = CreateArchiveFileSystem(archiveFile);
            usage = new FileSystemUsage
            {
                Owner = this,
                FileSystem = archiveFs,
                ArchiveFile = archiveFile
            };
            _usedArchives[archiveFile] = usage;
            //System.Diagnostics.Debug.WriteLine("Open archives: " + _usedArchives.Count);
            return usage;
        }

        protected abstract IFileSystem CreateArchiveFileSystem(File archiveFile);

        public class DummyDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }

        public class FileSystemReference : IDisposable
        {
            public FileSystemReference(FileSystemUsage usage)
            {
                Usage = usage;
            }

            public FileSystemUsage Usage { get; set; }

            public IFileSystem FileSystem => Usage.FileSystem;

            public void Dispose()
            {
                Usage.Owner.UnuseFileSystem(this);
            }
        }

        public class FileSystemUsage
        {
            public FileSystemUsage()
            {
                References = new LinkedList<FileSystemReference>();
            }

            public SeamlessArchiveFileSystem Owner { get; set; }
            public File ArchiveFile { get; set; }
            public IFileSystem FileSystem { get; set; }
            public ICollection<FileSystemReference> References { get; set; }
        }

        public class SafeReferenceStream : Stream
        {
            private readonly FileSystemReference _reference;
            private readonly Stream _stream;

            public SafeReferenceStream(Stream stream, FileSystemReference reference)
            {
                _stream = stream;
                _reference = reference;
            }

            public override bool CanRead => _stream.CanRead;

            public override bool CanSeek => _stream.CanSeek;

            public override bool CanWrite => _stream.CanWrite;

            public override long Length => _stream.Length;

            public override long Position
            {
                get => _stream.Position;
                set => _stream.Position = value;
            }

            public override void Flush()
            {
                _stream.Flush();
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                return _stream.Seek(offset, origin);
            }

            public override void SetLength(long value)
            {
                _stream.SetLength(value);
            }

            public override int Read(byte[] buffer, int offset, int count)
            {
                return _stream.Read(buffer, offset, count);
            }

            public override void Write(byte[] buffer, int offset, int count)
            {
                _stream.Write(buffer, offset, count);
            }

            public override void Close()
            {
                _stream.Close();
                _reference.Dispose();
            }
        }

        #region Not implemented

        public Stream CreateFile(FileSystemPath path)
        {
            var r = Refer(path);
            var s = r.FileSystem.CreateFile(GetRelativePath(path));
            return new SafeReferenceStream(s, r);
        }

        public void CreateDirectory(FileSystemPath path)
        {
            using (var r = Refer(path))
            {
                r.FileSystem.CreateDirectory(GetRelativePath(path));
            }
        }

        public void Delete(FileSystemPath path)
        {
            using (var r = Refer(path))
            {
                r.FileSystem.Delete(GetRelativePath(path));
            }
        }

        #endregion
    }
}