CSC = $(MCS) -debug
CSC_FLAGS = -target:exe $(BEAGLE_DEFINES)

if ENABLE_AVAHI
CSC_FLAGS += -define:ENABLE_AVAHI
endif

ASSEMBLY_NAME = Beagle.Daemon
ASSEMBLY = $(ASSEMBLY_NAME).exe

WRAPPER_IN = wrapper.in
WRAPPER_SED = sed 					\
	-e "s|\@prefix\@|$(prefix)|g"			\
	-e "s|\@pkglibdir\@|$(pkglibdir)|g"		\
	-e "s|\@libdir\@|$(libdir)|g"			\
	-e "s|\@evolibdir\@|$(EVO_LIBDIR)|g"		\
	-e "s|\@gacprefix\@|$(GAC_PREFIX)|g"		\
	-e "s|\@bash\@|$(BASH)|"			\
	-e "s|\@wrapper\@|$@|g"

DAEMON_WRAPPER = beagled
DAEMON_WRAPPER_IN = beagled.in
DAEMON_TARGET = BeagleDaemon.exe

$(DAEMON_WRAPPER): $(srcdir)/$(DAEMON_WRAPPER_IN)
	$(WRAPPER_SED) -e "s|\@target\@|$(ASSEMBLY)|g" < $(srcdir)/$(DAEMON_WRAPPER_IN) > $@
	chmod +x $(DAEMON_WRAPPER)

ASSEMBLY_SOURCES = \
	$(srcdir)/AssemblyInfo.cs \
	$(srcdir)/Beagle.Daemon/BeagleDaemon.cs \
	$(srcdir)/Beagle.Daemon/RemoteIndexer.cs

ASSEMBLY_REFERENCES = \
	-r:../Beagle.Util/Beagle.Util.dll \
	-r:../Beagle/Beagle.dll \
	-r:../Beagle.Filters/Beagle.Filters.dll \
	-r:../Beagle.Engine/Beagle.Engine.dll \
	$(BEAGLED_LIBS) \
	$(SHARPZIPLIB_LIBS) \
	$(NDESK_DBUS_GLIB_LIBS) \
	-r:Mono.Posix

$(ASSEMBLY): $(ASSEMBLY_SOURCES)
	$(CSC) -out:$@ $(CSC_FLAGS) $(ASSEMBLY_SOURCES) $(ASSEMBLY_REFERENCES)

all: $(ASSEMBLY) $(DAEMON_WRAPPER)

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

