CSC = $(MCS) -debug

ASSEMBLY_NAME = Beagle
ASSEMBLY = $(ASSEMBLY_NAME).dll

CSFLAGS = -target:library $(BEAGLE_DEFINES)

if ENABLE_AVAHI
CSFLAGS += -define:ENABLE_AVAHI
endif

ASSEMBLY_SOURCES = \
	AssemblyInfo.cs \
	Beagle/Hit.cs \
	Beagle/IIndexableGenerator.cs \
	Beagle/Indexable.cs \
	Beagle/IndexingService.cs \
	Beagle/IndexingStatusResponse.cs \
	Beagle/InformationalMessagesRequest.cs \
	Beagle/Message.cs \
	Beagle/Property.cs \
	Beagle/Query.cs \
	Beagle/QueryPart.cs \
	Beagle/QueryResponses.cs \
	Beagle/QueryableStatus.cs \
	Beagle/RemoteControl.cs \
	Beagle/Snippet.cs \
	Beagle/TcpTransport.cs \
	Beagle/Transport.cs \
	Beagle/UnixTransport.cs \
	Beagle/Versioned.cs

ASSEMBLY_REFERENCES = \
	-r:../Beagle.Util/Beagle.Util.dll \
	$(BEAGLED_LIBS) \
	$(SHARPZIPLIBS_LIBS) \
	-r:Mono.Posix

$(ASSEMBLY): $(ASSEMBLY_SOURCES)
	$(CSC) -out:$@ $(CSFLAGS) $(ASSEMBLY_SOURCES) $(ASSEMBLY_REFERENCES)

all: $(ASSEMBLY)

install-data-local: $(ASSEMBLY)
	$(mkinstalldirs) $(DESTDIR)$(pkglibdir)
	$(INSTALL_DATA) $(ASSEMBLY) $(ASSEMBLY).mdb $(DESTDIR)$(pkglibdir)

uninstall-local:
	rm -f $(DESTDIR)$(pkglibdir)/$(ASSEMBLY) $(DESTDIR)$(pkglibdir)/$(ASSEMBLY).mdb

EXTRA_DIST = \
	$(ASSEMBLY_SOURCES)

CLEANFILES = \
	$(ASSEMBLY) \
	$(ASSEMBLY).mdb

