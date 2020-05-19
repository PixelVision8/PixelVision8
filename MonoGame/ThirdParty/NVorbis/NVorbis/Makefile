SOURCES=ACache.cs DataPacket.cs Huffman.cs IPacketProvider.cs IVorbisStreamStatus.cs \
	Mdct.cs RingBuffer.cs Utils.cs Ogg/OggContainerReader.cs Ogg/OggPacket.cs \
	Ogg/OggPacketReader.cs Ogg/OggPageFlags.cs VorbisCodebook.cs VorbisFloor.cs \
	VorbisMapping.cs VorbisMode.cs VorbisReader.cs VorbisResidue.cs \
	VorbisStreamDecoder.cs VorbisTime.cs Properties/AssemblyInfo.cs
PROJECT=NVorbis.csproj
CONFIG=Release
OUTPUT_PATH=../bin
OUTPUT=$(OUTPUT_PATH)/NVorbis.dll
DOC_XML=$(OUTPUT_PATH)/NVorbis.XML

all: $(OUTPUT) $(DOC_XML)

$(DOC_XML): $(OUTPUT)

$(OUTPUT): $(SOURCES) $(PROJECT)
	xbuild /property:Configuration=$(CONFIG) $(PROJECT)

clean:
	rm -rf obj
	rm -rf bin
	rm -f $(OUTPUT)
	rm -f $(DOC_XML)

rebuild:
	xbuild /target:Rebuild /property:Configuration=$(CONFIG) $(PROJECT)
