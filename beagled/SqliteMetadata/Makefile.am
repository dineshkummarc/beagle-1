
# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = mcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -debug -d:DEBUG
ASSEMBLY = ./bin/Debug/SqlMeta.dll
COMPILE_TARGET = library
PC_FILES_IN = beagle_meta.SqlMeta.pc.in
PROJECT_REFERENCES = 
BUILD_DIR = ./bin/Debug
endif
if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = mcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 "-define:BLA_! ENABLE_SUPERBLA_DELUXE"

ASSEMBLY = ./bin/Release/SqlMeta.dll
COMPILE_TARGET = library
PC_FILES_IN = beagle_meta.SqlMeta.pc.in
PROJECT_REFERENCES = 
BUILD_DIR = ./bin/Release
endif

	
all: $(ASSEMBLY)

FILES = 

DATA_FILES = 

RESOURCES = 

EXTRAS = 

REFERENCES =  \
	-r:System

DLL_REFERENCES = 

$(ASSEMBLY) $(ASSEMBLY).mdb: $(build_sources) $(build_resources) $(build_datafiles) 
	mkdir -p $(dir $(ASSEMBLY))		

	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$@ -target:$(COMPILE_TARGET) $(build_sources) $(build_resources_embed) $(build_references_ref)

include $(top_srcdir)/Makefile.include
