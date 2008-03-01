SUBDIRS = src 

#data

if BUILD_DOCS
SUBDIRS +=			\
	doc
endif

#if ENABLE_GUI
#SUBDIRS +=			\
#	po			\
#	search			\
#	ImLogViewer
#endif

#SUBDIRS +=			\
#	firefox-extension	\
#	tools			\
#	conf-data

#if ENABLE_EPIPHANY_EXTENSION
#SUBDIRS += epiphany-extension
#endif

#if ENABLE_BLUDGEON
#SUBDIRS += bludgeon
#endif

#if ENABLE_THUNDERBIRD
#SUBDIRS += thunderbird-extension
#endif

EXTRA_DIST = 			\
	HACKING			\
	intltool-extract.in	\
	intltool-merge.in	\
	intltool-update.in	\
	$(pkgconfig_DATA) 	\
	beagle.spec

DISTCHECK_CONFIGURE_FLAGS = --disable-epiphany-extension-install --enable-docs

DISTCLEANFILES = 		\
	intltool-extract	\
	intltool-merge		\
	intltool-update		\
	$(pkgconfig_DATA)
