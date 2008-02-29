# Warnings we don't want to see
# 0169 = The private field is never used
# 0414 = The private field is assigned but its value is never used

CSC = $(MCS) -debug -nowarn:0169,0414
CSFLAGS = -target:library

ASSEMBLY_NAME = Beagle.Util
ASSEMBLY = $(ASSEMBLY_NAME).dll
ASSEMBLY_CONFIG = $(UTIL_TARGET).config

#UI_UTIL_TARGET = Beagle.UiUtil.dll
#UI_UTIL_TARGET_CONFIG = $(UI_UTIL_TARGET).config

if OS_LINUX
CSFLAGS += -define:OS_LINUX
endif

if OS_FREEBSD
CSFLAGS += -define:OS_FREEBSD
endif

EXTSTR = Beagle.Util/ExternalStringsHack.cs
EXTSTR_IN = $(EXTSTR).in
$(EXTSTR): $(EXTSTR_IN)
	sed	-e "s|\@prefix\@|$(prefix)|g"			\
		-e "s|\@sysconfdir\@|$(sysconfdir)|g"		\
		-e "s|\@pkglibdir\@|$(pkglibdir)|g"		\
		-e "s|\@pkgdatadir\@|$(pkgdatadir)|g"		\
		-e "s|\@localstatedir\@|$(localstatedir)|g"	\
		-e "s|\@datadir\@|$(datadir)|g"			\
		-e "s|\@VERSION\@|$(VERSION)|g"			\
		-e "s|\@GNOME_PREFIX\@|$(GNOME_PREFIX)|g"	\
		-e "s|\@KDE_PREFIX\@|$(KDE_PREFIX)|g"		\
		-e "s|\@SQLITE_MAJ_VER\@|$(SQLITE_MAJ_VER)|g"	\
		< $(EXTSTR_IN) > $@

ASSEMBLY_SOURCES = \
	Beagle.Util/ArrayFu.cs \
	Beagle.Util/Battery.cs \
	Beagle.Util/BetterBitArray.cs \
	Beagle.Util/Camel.cs \
	Beagle.Util/Config.cs \
	Beagle.Util/DateTimeUtil.cs \
	Beagle.Util/DelayedTextWriter.cs \
	Beagle.Util/DigikamTags.cs \
	Beagle.Util/DirectoryWalker.cs \
	Beagle.Util/Evolution.cs \
	Beagle.Util/ExceptionHandlingThread.cs \
	Beagle.Util/ExtendedAttribute.cs \
	Beagle.Util/FileAdvise.cs \
	Beagle.Util/FileSystem.cs \
	Beagle.Util/FrequencyStatistics.cs \
	Beagle.Util/FSpotTools.cs \
	Beagle.Util/GuidFu.cs \
	Beagle.Util/HtmlRemovingReader.cs \
	Beagle.Util/ICalParser.cs \
	Beagle.Util/ImBuddy.cs \
	Beagle.Util/Inotify.cs \
	Beagle.Util/KdeUtils.cs \
	Beagle.Util/KonqHistoryUtil.cs \
	Beagle.Util/LineReader.cs \
	Beagle.Util/Log.cs \
	Beagle.Util/Logger.cs \
	Beagle.Util/MultiReader.cs \
	Beagle.Util/NautilusTools.cs \
	Beagle.Util/NetworkService.cs \
	Beagle.Util/Note.cs \
	Beagle.Util/OperaHistory.cs \
	Beagle.Util/Password.cs \
	Beagle.Util/PathFinder.cs \
	Beagle.Util/PullingReader.cs \
	Beagle.Util/ReflectionFu.cs \
	Beagle.Util/QueryKeywordMapping.cs \
	Beagle.Util/SafeProcess.cs \
	Beagle.Util/Scheduler.cs \
	Beagle.Util/SmallIntArray.cs \
	Beagle.Util/Stopwatch.cs \
	Beagle.Util/StringFu.cs \
	Beagle.Util/StringMatcher.cs \
	Beagle.Util/SystemInformation.cs \
	Beagle.Util/SystemPriorities.cs \
	Beagle.Util/TeeTextWriter.cs \
	Beagle.Util/ThreadPond.cs	\
	Beagle.Util/Timeline.cs \
	Beagle.Util/TopScores.cs \
	Beagle.Util/TypeCacheAttribute.cs \
	Beagle.Util/UnclosableStream.cs \
	Beagle.Util/UriFu.cs \
	Beagle.Util/VersionFu.cs \
	Beagle.Util/XdgMime.cs \
	Beagle.Util/XmlFu.cs \
	F-Spot/Bim.cs \
	F-Spot/BitConverter.cs \
	F-Spot/Exif.cs \
	F-Spot/IptcFile.cs \
	F-Spot/JpegHeader.cs \
	F-Spot/MetadataStore.cs \
	F-Spot/PngFile.cs \
	F-Spot/PixbufUtils.cs \
	F-Spot/Tiff.cs \
	F-Spot/XmpFile.cs \
	Hal/AccessControl.cs \
	Hal/CPUFreq.cs \
	Hal/Context.cs \
	Hal/Device.cs \
	Hal/IAccessControl.cs \
	Hal/ICPUFreq.cs \
	Hal/IDevice.cs \
	Hal/IKillSwitch.cs \
	Hal/ILaptopPanel.cs \
	Hal/ILightSensor.cs \
	Hal/IManager.cs \
	Hal/IStorage.cs \
	Hal/ISystemPowerManagement.cs \
	Hal/IVolume.cs \
	Hal/IVolumeCrypto.cs \
	Hal/KillSwitch.cs \
	Hal/LaptopPanel.cs \
	Hal/LightSensor.cs \
	Hal/Manager.cs \
	Hal/PropertyModification.cs \
	Hal/Storage.cs \
	Hal/SystemPowerManagement.cs \
	Hal/Volume.cs \
	Hal/VolumeCrypto.cs \
	SemWeb/NamespaceManager.cs \
	SemWeb/Util.cs \
	SemWeb/UriMap.cs \
	SemWeb/Resource.cs \
	SemWeb/Statement.cs \
	SemWeb/Store.cs \
	SemWeb/MemoryStore.cs \
	SemWeb/SQLStore.cs \
	SemWeb/RdfReader.cs \
	SemWeb/RdfXmlReader.cs \
	SemWeb/N3Reader.cs \
	SemWeb/RdfWriter.cs \
	SemWeb/RdfXmlWriter.cs \
	SemWeb/N3Writer.cs \
	SemWeb/RSquary.cs \
	SemWeb/LiteralFilters.cs \
	SemWeb/Query.cs \
	SemWeb/Inference.cs \
	SemWeb/RDFS.cs \
	SemWeb/Algos.cs \
	SemWeb/Remote.cs \
	SemWeb/XPathSemWebNavigator.cs

ASSEMBLY_REFERENCES = \
	-r:System.Data \
	-r:Mono.Data.Sqlite \
	-r:Mono.Posix \
	-r:System.Web \
	-r:ICSharpCode.SharpZipLib \
	$(BEAGLED_LIBS) \
	$(NDESK_DBUS_GLIB_LIBS)

if ENABLE_THUNDERBIRD
ASSEMBLY_SOURCES += Beagle.Util/Thunderbird.cs
endif

if ENABLE_AVAHI
CSFLAGS += -define:ENABLE_AVAHI
ASSEMBLY_SOURCES += Beagle.Util/AvahiBrowser.cs
ASSEMBLY_REFERENCES += $(AVAHI_SHARP_LIBS)
endif

if HAS_LIBCHM 
ASSEMBLY_SOURCES += Beagle.Util/ChmFile.cs
endif

if ENABLE_INOTIFY
CSFLAGS += -define:ENABLE_INOTIFY
endif

$(ASSEMBLY): $(ASSEMBLY_SOURCES) $(EXTSTR)
	$(CSC) -unsafe -out:$@ $(CSFLAGS) $(ASSEMBLY_SOURCES) $(EXTSTR) $(ASSEMBLY_REFERENCES)

all: $(ASSEMBLY)

install-data-local: $(ASSEMBLY)
	$(mkinstalldirs) $(DESTDIR)$(pkglibdir)
	$(INSTALL_DATA) $(ASSEMBLY) $(ASSEMBLY).mdb $(ASSEBMLY_CONFIG) $(DESTDIR)$(pkglibdir)

uninstall-local:
	rm -f $(DESTDIR)$(pkglibdir)/$(ASSEMBLY) $(DESTDIR)$(pkglibdir)/$(ASSEMBLY).mdb $(DESTDIR)$(pkglibdir)/$(ASSEMBLY_CONFIG)

COND_CS_FILES =				\
	AvahiBrowser.cs	\
	ChmFile.cs		\
	Thunderbird.cs

EXTRA_DIST = \
	AssemblyInfo.cs.in \
	$(EXTSTR_IN) \
	$(ASSEMBLY_SOURCES) \
	$(COND_CS_FILES) \
	Util.dll.config.in

CLEANFILES = \
	$(EXTSTR) \
	$(UTIL_TARGET) \
	$(UTIL_TARGET).mdb

