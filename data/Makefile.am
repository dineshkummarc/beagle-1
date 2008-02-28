# Require at least automake 1.8
AUTOMAKE_OPTIONS = 1.8

pkgconfigdir = $(libdir)/pkgconfig
pkgconfig_DATA = beagle-0.0.pc beagle-daemon.pc

#if ENABLE_GUI
#pkgconfig_DATA += beagle-ui-0.0.pc
#endif

EXTRA_DIST = \
	$(pkgconfig_DATA) \
	beagle.spec

DISTCLEANFILES = \
	$(pkgconfig_DATA)
