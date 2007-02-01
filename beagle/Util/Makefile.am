# Warnings we don't want to see
# 0169 = The private field is never used
# 0414 = The private field is assigned but its value is never used

SUBDIRS = Snowball

CSC = $(MCS) -debug -nowarn:0169,0414
CSFLAGS = -target:library

UTIL_TARGET = Util.dll
UTIL_TARGET_CONFIG = $(UTIL_TARGET).config

GENERATEFP_TARGET = GenerateFingerprint.exe

UI_UTIL_TARGET = UiUtil.dll
UI_UTIL_TARGET_CONFIG = $(UI_UTIL_TARGET).config


if OS_LINUX
CSFLAGS += -define:OS_LINUX
endif

if OS_FREEBSD
CSFLAGS += -define:OS_FREEBSD
endif

EXTSTR = ExternalStringsHack.cs
EXTSTR_IN = $(srcdir)/$(EXTSTR).in
$(EXTSTR): $(EXTSTR_IN)
	sed	-e "s|\@prefix\@|$(prefix)|g"			\
		-e "s|\@sysconfdir\@|$(sysconfdir)|g"		\
		-e "s|\@pkglibdir\@|$(pkglibdir)|g"		\
		-e "s|\@pkgdatadir\@|$(pkgdatadir)|g"		\
		-e "s|\@langfpdir\@|$(langfpdir)|g"		\
		-e "s|\@localstatedir\@|$(localstatedir)|g"	\
		-e "s|\@VERSION\@|$(VERSION)|g"			\
		-e "s|\@GNOME_PREFIX\@|$(GNOME_PREFIX)|g"	\
		-e "s|\@KDE_PREFIX\@|$(KDE_PREFIX)|g"		\
		-e "s|\@SQLITE_MAJ_VER\@|$(SQLITE_MAJ_VER)|g"	\
		< $(EXTSTR_IN) > $@

UTIL_CSFILES = 			        	\
	$(srcdir)/ArrayFu.cs              	\
	$(srcdir)/BetterBitArray.cs		\
	$(srcdir)/camel.cs              	\
	$(srcdir)/CommandLineFu.cs		\
	$(srcdir)/Conf.cs			\
	$(srcdir)/DateTimeUtil.cs		\
	$(srcdir)/DelayedTextWriter.cs		\
	$(srcdir)/DigikamTags.cs		\
	$(srcdir)/DirectoryWalker.cs		\
	$(srcdir)/Evolution.cs			\
	$(srcdir)/ExceptionHandlingThread.cs	\
	$(srcdir)/ExtendedAttribute.cs  	\
	$(srcdir)/FileAdvise.cs			\
	$(srcdir)/FileSystem.cs			\
	$(srcdir)/FrequencyStatistics.cs	\
	$(srcdir)/FSpotTools.cs         	\
	$(srcdir)/GuidFu.cs			\
	$(srcdir)/ICalParser.cs              	\
	$(srcdir)/ImBuddy.cs              	\
	$(srcdir)/ImLog.cs              	\
	$(srcdir)/Inotify.cs			\
	$(srcdir)/KdeUtils.cs			\
	$(srcdir)/KonqHistoryUtil.cs		\
	$(srcdir)/LineReader.cs			\
	$(srcdir)/Log.cs			\
	$(srcdir)/Logger.cs             	\
	$(srcdir)/MultiReader.cs        	\
	$(srcdir)/NautilusTools.cs      	\
	$(srcdir)/Note.cs               	\
	$(srcdir)/PathFinder.cs			\
	$(srcdir)/PullingReader.cs      	\
	$(srcdir)/ReflectionFu.cs		\
	$(srcdir)/SafeProcess.cs		\
	$(srcdir)/Scheduler.cs			\
	$(srcdir)/SmallIntArray.cs		\
	$(srcdir)/Stopwatch.cs			\
	$(srcdir)/StringFu.cs           	\
	$(srcdir)/StringMatcher.cs		\
	$(srcdir)/SystemInformation.cs		\
	$(srcdir)/SystemPriorities.cs		\
	$(srcdir)/TeeTextWriter.cs		\
	$(srcdir)/ThreadPond.cs			\
	$(srcdir)/Timeline.cs			\
	$(srcdir)/TopScores.cs			\
	$(srcdir)/TypeCacheAttribute.cs		\
	$(srcdir)/UnclosableStream.cs		\
	$(srcdir)/UriFu.cs			\
	$(srcdir)/XdgMime.cs			\
	$(srcdir)/XmlFu.cs			\
	$(srcdir)/F-Spot/Bim.cs			\
	$(srcdir)/F-Spot/BitConverter.cs		\
	$(srcdir)/F-Spot/Exif.cs			\
	$(srcdir)/F-Spot/IptcFile.cs		\
	$(srcdir)/F-Spot/JpegHeader.cs		\
	$(srcdir)/F-Spot/MetadataStore.cs	\
	$(srcdir)/F-Spot/PngFile.cs		\
	$(srcdir)/F-Spot/PixbufUtils.cs		\
	$(srcdir)/F-Spot/Tiff.cs			\
	$(srcdir)/F-Spot/XmpFile.cs		\
	$(srcdir)/Snowball/csharp/Among.cs      \
	$(srcdir)/Snowball/csharp/SnowballProgram.cs 	\
	$(srcdir)/SemWeb/AssemblyInfo.cs        \
	$(srcdir)/SemWeb/NamespaceManager.cs    \
	$(srcdir)/SemWeb/Util.cs                \
	$(srcdir)/SemWeb/UriMap.cs              \
	$(srcdir)/SemWeb/Resource.cs            \
	$(srcdir)/SemWeb/Statement.cs           \
	$(srcdir)/SemWeb/Store.cs               \
	$(srcdir)/SemWeb/MemoryStore.cs         \
	$(srcdir)/SemWeb/SQLStore.cs            \
	$(srcdir)/SemWeb/RdfReader.cs           \
	$(srcdir)/SemWeb/RdfXmlReader.cs        \
	$(srcdir)/SemWeb/N3Reader.cs            \
	$(srcdir)/SemWeb/RdfWriter.cs           \
	$(srcdir)/SemWeb/RdfXmlWriter.cs        \
	$(srcdir)/SemWeb/N3Writer.cs            \
	$(srcdir)/SemWeb/RSquary.cs             \
	$(srcdir)/SemWeb/LiteralFilters.cs      \
	$(srcdir)/SemWeb/Query.cs               \
	$(srcdir)/SemWeb/Inference.cs           \
	$(srcdir)/SemWeb/RDFS.cs                \
	$(srcdir)/SemWeb/Algos.cs               \
	$(srcdir)/SemWeb/Remote.cs              \
	$(srcdir)/SemWeb/XPathSemWebNavigator.cs\
	$(srcdir)/TextCat/Fingerprint.cs 	\
	$(srcdir)/TextCat/TextCategorizer.cs 	\
	$(srcdir)/Snowball/csharp/SnowballProgram.cs \
	$(srcdir)/Snowball/csharp/Among.cs 	\
	$(srcdir)/PropertyKeywordAttribute.cs 

if ENABLE_THUNDERBIRD
UTIL_CSFILES +=				\
	$(srcdir)/Mork.cs		\
	$(srcdir)/Thunderbird.cs
endif

if HAS_LIBCHM 
UTIL_CSFILES +=					\
	$(srcdir)/ChmFile.cs
endif

UTIL_ASSEMBLIES =				\
	$(BEAGLED_LIBS)				\
	-r:System.Data				\
	-r:Mono.Data.SqliteClient		\
	-r:Mono.Posix				\
	-r:System.Web				\
	-r:ICSharpCode.SharpZipLib

GENERATEFP_CSFILES = ./TextCat/GenerateFingerprint.cs

BUILDFP_DIR = $(top_builddir)/Util/TextCat/sample_texts
BUILDFP_TARGET = .langs_built
TEXTCAT_LANGS = af ar be bg br bs ca cs cy da de el en eo es et eu fa fi \
		fr fy gv he hi hr hu hy id is it ja ka ko la lt lv mf mr \
		ms nb nl pl pt qu ro ru sk sl sq sr sv sw th tr uk vi yi \
		zh 

textcat_paths =  $(addprefix $(BUILDFP_DIR)/, $(TEXTCAT_LANGS))
textcat_sources =  $(addsuffix .txt, $(textcat_paths) )
textcat_target_files =  $(addsuffix .fp, $(textcat_paths) )
textcat_target_filenames =  $(addsuffix .fp, $(TEXTCAT_LANGS) )
langfpdir = $(pkgdatadir)/lang_fp

SNOWBALL_DIR = $(top_builddir)/Util/Snowball
SNOWBALL_LANGS = de da en es fi fr hu it nb nl nb nn pt ro ru sv 
SNOWBALL_TARGET = .snowball_built
SNOWBALL_COMPILER = ./Snowball/snowball
SNOWBALL_ALGORITHMS = ./Snowball/algorithms
SNOWBALL_LANGCODEMAP = $(SNOWBALL_ALGORITHMS)/langcode_mapping

# HACK: I had a much better way to do this involving symlinks, but turns out
# you can't symlink something in a patch
snowball_paths = $(addprefix $(SNOWBALL_ALGORITHMS)/, $(SNOWBALL_LANGS))
snowball_sources = $(addprefix $(SNOWBALL_ALGORITHMS)/, $(shell cat "$(SNOWBALL_LANGCODEMAP)" | cut -d ' ' -f 2))
snowball_target_files = $(addsuffix .cs, $(snowball_paths) )

if ENABLE_INOTIFY
INOTIFY_EXE = Inotify.exe
CSFLAGS += -define:ENABLE_INOTIFY
else
INOTIFY_EXE =
endif

UTIL_CSFILES += $(snowball_target_files)

UTIL_TARGET_FILES = $(UTIL_CSFILES) $(EXTSTR)

$(UTIL_TARGET): $(UTIL_TARGET_FILES)
	$(CSC) -unsafe -out:$@ $(CSFLAGS) $^ $(UTIL_ASSEMBLIES)

UI_UTIL_CSFILES =				\
	$(srcdir)/CompatFileChooser.cs		\
	$(srcdir)/GnomeFu.cs              	\
	$(srcdir)/GtkUtils.cs			\
	$(srcdir)/HigMessageDialog.cs		\
	$(srcdir)/XKeybinder.cs

UI_UTIL_ASSEMBLIES =				\
	$(BEAGLE_UI_LIBS)			\
	-r:Util.dll

if ENABLE_GALAGO
UI_UTIL_CSFILES +=					\
	$(srcdir)/Galago.cs

UI_UTIL_ASSEMBLIES += 				\
	$(GALAGO_LIBS)
endif

$(UI_UTIL_TARGET): $(UI_UTIL_CSFILES) $(UTIL_TARGET)
	$(CSC) -unsafe -out:$@ $(CSFLAGS) $(UI_UTIL_CSFILES) $(UI_UTIL_ASSEMBLIES)

INOTIFY_TEST_CSFILES =				\
	$(srcdir)/Inotify.cs			\
	$(srcdir)/Log.cs			\
	$(srcdir)/Logger.cs			\
	$(srcdir)/DelayedTextWriter.cs		\
	$(srcdir)/TeeTextWriter.cs		\
	$(srcdir)/DirectoryWalker.cs		\
	$(srcdir)/FileSystem.cs			\
	$(srcdir)/ExceptionHandlingThread.cs

Inotify.exe: $(INOTIFY_TEST_CSFILES)
	$(CSC) -unsafe -out:$@ $^ -r:Mono.Posix -define:INOTIFY_TEST -define:ENABLE_INOTIFY

$(GENERATEFP_TARGET): $(GENERATEFP_CSFILES) $(UTIL_TARGET)
	$(CSC) -unsafe -out:$@ $(GENERATEFP_CSFILES) -r:Mono.Posix -r:$(UTIL_TARGET)

$(BUILDFP_TARGET): $(textcat_sources) $(GENERATEFP_TARGET)
	$(MONO) $(GENERATEFP_TARGET) $(textcat_sources)
	touch $@

$(SNOWBALL_TARGET): $(snowball_sources)
	for f in $(SNOWBALL_LANGS); do \
		$(SNOWBALL_COMPILER) 	$(SNOWBALL_ALGORITHMS)/`cat $(SNOWBALL_LANGCODEMAP) | grep "^$$f" | cut -f 2 -d ' '` \
					-o Snowball/algorithms/$$f.cs -csharp -n "Stemmer_$$f"; \
	done
	touch $@

ALL_TARGETS =			\
	$(SNOWBALL_TARGET)	\
	$(UTIL_TARGET)		\
	$(GENERATEFP_TARGET)	\
	$(BUILDFP_TARGET)	\
	$(INOTIFY_EXE)

if ENABLE_GUI
ALL_TARGETS += $(UI_UTIL_TARGET)
endif

all: $(ALL_TARGETS)

install-data-local: $(ALL_TARGETS)
	$(mkinstalldirs) $(DESTDIR)$(pkglibdir)
	$(mkinstalldirs) $(DESTDIR)$(pkgdatadir)
	$(mkinstalldirs) $(DESTDIR)$(langfpdir)
	$(INSTALL_DATA) $(UTIL_TARGET) $(UTIL_TARGET).mdb $(UTIL_TARGET_CONFIG) $(DESTDIR)$(pkglibdir)
	$(INSTALL_DATA) $(textcat_target_files) $(DESTDIR)$(langfpdir) 
if ENABLE_GUI
	$(INSTALL_DATA) $(UI_UTIL_TARGET) $(UI_UTIL_TARGET).mdb $(srcdir)/$(UI_UTIL_TARGET_CONFIG) $(DESTDIR)$(pkglibdir)
endif

uninstall-local:
	rm -f $(DESTDIR)$(pkglibdir)/$(UTIL_TARGET) $(DESTDIR)$(pkglibdir)/$(UTIL_TARGET).mdb $(DESTDIR)$(pkglibdir)/$(UTIL_TARGET_CONFIG)
	rm -f $(DESTDIR)$(pkglibdir)/$(UI_UTIL_TARGET) $(DESTDIR)$(pkglibdir)/$(UI_UTIL_TARGET).mdb $(DESTDIR)$(pkglibdir)/$(UI_UTIL_TARGET_CONFIG)
	rm -f $(addprefix $(DESTDIR)$(langfpdir)/, $(textcat_target_filenames)) 


COND_CS_FILES =				\
	$(srcdir)/ChmFile.cs		\
	$(srcdir)/Galago.cs		\
	$(srcdir)/Mork.cs		\
	$(srcdir)/Thunderbird.cs

EXTRA_DIST =			\
	$(EXTSTR_IN)		\
	$(UTIL_CSFILES)		\
	$(UI_UTIL_CSFILES)	\
	$(COND_CS_FILES)	\
	$(textcat_sources)	\
	UiUtil.dll.config	\
	Util.dll.config.in	\
	inotify-test

CLEANFILES =			\
	$(EXTSTR)		\
	$(UTIL_TARGET)		\
	$(UTIL_TARGET).mdb	\
	$(UI_UTIL_TARGET)	\
	$(UI_UTIL_TARGET).mdb	\
	$(GENERATEFP_TARGET)	\
	$(BUILDFP_TARGET)	\
	$(SNOWBALL_TARGET)	\
	$(textcat_target_files) \
	$(snowball_target_files)	\
	Inotify.exe		\
	Inotify.exe.mdb

