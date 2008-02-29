CSC = $(MCS) -debug
CSFLAGS = -target:library $(BEAGLE_DEFINES)

ASSEMBLY_NAME = Beagle.Filters
ASSEMBLY = $(ASSEMBLY_NAME).dll

FILTERDIR = $(pkglibdir)/Filters

# HtmlAgilityPack found at http://sharptoolbox.madgeek.com/
# RTF is a local copy of http://svn.myrealbox.com/viewcvs/trunk/mcs/class/Managed.Windows.Forms/System.Windows.Forms.RTF/

include TagLib/TagLib.sources
include 7zip/7zip.sources

ASSEMBLY_SOURCES = \
	AssemblyInfo.cs	\
	Beagle.Filters/Filter.cs	\
	Beagle.Filters/FilterFlavor.cs	\
	Beagle.Filters/FilterFactory.cs	\
	Beagle.Filters/FilterAbiword.cs	\
	Beagle.Filters/FilterArchive.cs	\
	Beagle.Filters/FilterAudio.cs	\
	Beagle.Filters/FilterBmp.cs		\
	Beagle.Filters/FilterBoo.cs		\
	Beagle.Filters/FilterC.cs		\
	Beagle.Filters/FilterCSharp.cs	\
	Beagle.Filters/FilterCpp.cs		\
	Beagle.Filters/FilterDeb.cs		\
	Beagle.Filters/FilterDesktop.cs	\
	Beagle.Filters/FilterDocbook.cs	\
	Beagle.Filters/FilterEbuild.cs	\
	Beagle.Filters/FilterExternal.cs	\
	Beagle.Filters/FilterEmpathyLog.cs	\
	Beagle.Filters/FilterFortran.cs	\
	Beagle.Filters/FilterGif.cs		\
	Beagle.Filters/FilterHtml.cs		\
	Beagle.Filters/FilterIgnore.cs	\
	Beagle.Filters/FilterImage.cs	\
	Beagle.Filters/FilterJava.cs		\
	Beagle.Filters/FilterJpeg.cs		\
	Beagle.Filters/FilterJs.cs		\
	Beagle.Filters/FilterKAddressBook.cs \
	Beagle.Filters/FilterKCal.cs		\
	Beagle.Filters/FilterKNotes.cs	\
	Beagle.Filters/FilterKOrganizer.cs	\
	Beagle.Filters/FilterKonqHistory.cs	\
	Beagle.Filters/FilterKopeteLog.cs	\
	Beagle.Filters/FilterLabyrinth.cs	\
	Beagle.Filters/FilterLisp.cs		\
	Beagle.Filters/FilterM3U.cs		\
	Beagle.Filters/FilterMPlayerVideo.cs	\
	Beagle.Filters/FilterMail.cs		\
	Beagle.Filters/FilterMan.cs		\
	Beagle.Filters/FilterMatlab.cs	\
	Beagle.Filters/FilterMonodoc.cs	\
	Beagle.Filters/FilterOpenOffice.cs	\
	Beagle.Filters/FilterPackage.cs	\
	Beagle.Filters/FilterPascal.cs	\
	Beagle.Filters/FilterPdf.cs		\
	Beagle.Filters/FilterPerl.cs		\
	Beagle.Filters/FilterPhp.cs		\
	Beagle.Filters/FilterPidginLog.cs	\
	Beagle.Filters/FilterPls.cs		\
	Beagle.Filters/FilterPng.cs		\
	Beagle.Filters/FilterPython.cs	\
	Beagle.Filters/FilterRPM.cs		\
	Beagle.Filters/FilterRTF.cs		\
	Beagle.Filters/FilterRuby.cs		\
	Beagle.Filters/FilterScilab.cs	\
	Beagle.Filters/FilterScribus.cs	\
	Beagle.Filters/FilterShellscript.cs	\
	Beagle.Filters/FilterSource.cs	\
	Beagle.Filters/FilterSpreadsheet.cs	\
	Beagle.Filters/FilterSvg.cs		\
	Beagle.Filters/FilterTeX.cs		\
	Beagle.Filters/FilterText.cs	        \
	Beagle.Filters/FilterTiff.cs		\
	Beagle.Filters/FilterTotem.cs	\
	Beagle.Filters/FilterVideo.cs	\
	Beagle.Filters/FilterXslt.cs \
	HtmlAgilityPack/crc32.cs		\
	HtmlAgilityPack/Header.cs		\
	HtmlAgilityPack/HtmlAttribute.cs	\
	HtmlAgilityPack/HtmlDocument.cs	\
	HtmlAgilityPack/HtmlEntity.cs		\
	HtmlAgilityPack/HtmlNode.cs		\
	HtmlAgilityPack/HtmlNodeNavigator.cs	\
	HtmlAgilityPack/MixedCodeDocument.cs	\
	HtmlAgilityPack/tools.cs \
	System.Windows.Forms.RTF/Charcode.cs		\
	System.Windows.Forms.RTF/Charset.cs		\
	System.Windows.Forms.RTF/CharsetFlags.cs	\
	System.Windows.Forms.RTF/CharsetType.cs	\
	System.Windows.Forms.RTF/ClassDelegate.cs	\
	System.Windows.Forms.RTF/Color.cs		\
	System.Windows.Forms.RTF/DestinationDelegate.cs \
	System.Windows.Forms.RTF/Font.cs		\
	System.Windows.Forms.RTF/KeysInit.cs		\
	System.Windows.Forms.RTF/KeyStruct.cs		\
	System.Windows.Forms.RTF/Major.cs		\
	System.Windows.Forms.RTF/Minor.cs		\
	System.Windows.Forms.RTF/RTF.cs		\
	System.Windows.Forms.RTF/RTFException.cs	\
	System.Windows.Forms.RTF/StandardCharCode.cs	\
	System.Windows.Forms.RTF/StandardCharName.cs	\
	System.Windows.Forms.RTF/Style.cs		\
	System.Windows.Forms.RTF/StyleElement.cs	\
	System.Windows.Forms.RTF/StyleType.cs		\
	System.Windows.Forms.RTF/TextMap.cs		\
	System.Windows.Forms.RTF/TokenClass.cs \
	$(7ZIP_CSFILES)

if ENABLE_GSF_SHARP
ASSEMBLY_SOURCES += FilterOle.cs FilterPPT.cs

if ENABLE_WV1
ASSEMBLY_SOURCES += FilterDOC.cs		
endif
endif

if HAS_LIBCHM
ASSEMBLY_SOURCES += FilterChm.cs
endif

ASSEMBLY_REFERENCES = \
	-r:../Beagle.Util/Beagle.Util.dll \
	-r:../Beagle/Beagle.dll \
	$(BEAGLED_LIBS) \
	$(SHARPZIPLIB_LIBS) \
	-r:Mono.Posix \
	-r:System.Web

if INTERNAL_TAGLIB
ASSEMBLY_SOURCES += $(TAGLIB_CSFILES)
else
ASSEMBLY_REFERENCES += $(TAGLIB_SHARP_LIBS)
endif

$(ASSEMBLY): $(ASSEMBLY_SOURCES)
	$(CSC) -out:$@ $(CSFLAGS) $(ASSEMBLY_SOURCES) $(ASSEMBLY_REFERENCES)

all: $(ASSEMBLY)

install-data-local: $(ASSEMBLY)
	$(mkinstalldirs) $(DESTDIR)$(FILTERDIR)
	$(INSTALL_DATA) $(ASSEMBLY) $(ASSEMBLY).mdb $(DESTDIR)$(FILTERDIR)

uninstall-local:
	rm -f $(DESTDIR)$(FILTERDIR)/$(ASSEMBLY)
	rm -f $(DESTDIR)$(FILTERDIR)/$(ASSEMBLY).mdb

COND_CS_FILES = \
	Beagle.Filters/FilterOle.cs \
	Beagle.Filters/FilterPPT.cs \
	Beagle.Filters/FilterDOC.cs \
	Beagle.Filters/FilterChm.cs

EXTRA_DIST = \
	$(ASSEMBLY_SOURCES) \
	$(COND_CS_FILES)

CLEANFILES = \
	$(ASSEMBLY) \
	$(ASSEMBLY).mdb

