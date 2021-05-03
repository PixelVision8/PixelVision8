const gulp = require('gulp');
let {run} = require('gulp-dotnet-cli');

gulp.task('build-pv8', ()=>{
  return gulp.src(process.env.PROJECT, {read: false})
      .pipe(run({
        additionalArgs: [process.env.DISKS + 'PixelVisionOS/', process.env.DISKS + 'APIExamples/'], // APIExamples ['-d', 'Disks/PixelVisionOS/', '-disk', 'Disks/APIExamples/']
        echo: true
    }));
});

gulp.task("run-pv8", gulp.series('build-pv8'));


gulp.task('build-csharp-run', ()=>{
  return gulp.src("./Projects/CSharpRunner/PixelVision8.CSharpRunner.csproj", {read: false})
      .pipe(run({
        additionalArgs: [],
        echo: true
    }));
});

gulp.task("run-csharp", gulp.series('build-csharp-run'));


gulp.task('build-lua-run', ()=>{
  return gulp.src("./Projects/LuaRunner/PixelVision8.LuaRunner.csproj", {read: false})
      .pipe(run({
        additionalArgs: [],
        echo: true
    }));
});

gulp.task("run-lua", gulp.series('build-lua-run'));


gulp.task('build-roslyn-run', ()=>{
  return gulp.src("./Projects/RoslynRunner/PixelVision8.RoslynRunner.csproj", {read: false})
      .pipe(run({
        additionalArgs: [],
        echo: true
    }));
});

gulp.task("run-roslyn", gulp.series('build-roslyn-run'));