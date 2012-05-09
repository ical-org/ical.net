
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = mcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG;TRACE"
ASSEMBLY = bin/Debug/DDay.Collections.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

DDAY_COLLECTIONS_DLL_MDB_SOURCE=bin/Debug/DDay.Collections.dll.mdb
DDAY_COLLECTIONS_DLL_MDB=$(BUILD_DIR)/DDay.Collections.dll.mdb
DDAY_COLLECTIONS_DLL_CONFIG_SOURCE=app.config

endif

if ENABLE_DEBUG_MIXED_PLATFORMS
ASSEMBLY_COMPILER_COMMAND = mcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG;TRACE"
ASSEMBLY = bin/Debug/DDay.Collections.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

DDAY_COLLECTIONS_DLL_MDB_SOURCE=bin/Debug/DDay.Collections.dll.mdb
DDAY_COLLECTIONS_DLL_MDB=$(BUILD_DIR)/DDay.Collections.dll.mdb
DDAY_COLLECTIONS_DLL_CONFIG_SOURCE=app.config

endif

if ENABLE_DEBUG_X86
ASSEMBLY_COMPILER_COMMAND = mcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:DEBUG;TRACE"
ASSEMBLY = bin/Debug/DDay.Collections.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Debug

DDAY_COLLECTIONS_DLL_MDB_SOURCE=bin/Debug/DDay.Collections.dll.mdb
DDAY_COLLECTIONS_DLL_MDB=$(BUILD_DIR)/DDay.Collections.dll.mdb
DDAY_COLLECTIONS_DLL_CONFIG_SOURCE=app.config

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = mcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ "-define:TRACE"
ASSEMBLY = bin/Release/DDay.Collections.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release

DDAY_COLLECTIONS_DLL_MDB=
DDAY_COLLECTIONS_DLL_CONFIG_SOURCE=app.config

endif

if ENABLE_RELEASE_MIXED_PLATFORMS
ASSEMBLY_COMPILER_COMMAND = mcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ "-define:TRACE"
ASSEMBLY = bin/Release/DDay.Collections.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release

DDAY_COLLECTIONS_DLL_MDB=
DDAY_COLLECTIONS_DLL_CONFIG_SOURCE=app.config

endif

if ENABLE_RELEASE_X86
ASSEMBLY_COMPILER_COMMAND = mcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ "-define:TRACE"
ASSEMBLY = bin/Release/DDay.Collections.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES = 
BUILD_DIR = bin/Release

DDAY_COLLECTIONS_DLL_MDB=
DDAY_COLLECTIONS_DLL_CONFIG_SOURCE=app.config

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(DDAY_COLLECTIONS_DLL_MDB) \
	$(DDAY_COLLECTIONS_DLL_CONFIG)  

LINUX_PKGCONFIG = \
	$(DDAY_COLLECTIONS_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	Constants.cs \
	Interfaces/IGroupedValueCollection.cs \
	Interfaces/IGroupedCollection.cs \
	Interfaces/Proxies/IGroupedValueListProxy.cs \
	Proxies/GroupedCollectionProxy.cs \
	Interfaces/IGroupedList.cs \
	Interfaces/IGroupedObject.cs \
	Interfaces/IGroupedValueList.cs \
	Interfaces/IMultiLinkedList.cs \
	Interfaces/IValueObject.cs \
	GroupedList.cs \
	GroupedValueList.cs \
	Proxies/GroupedValueListProxy.cs \
	MultiLinkedList.cs \
	Enumerators/GroupedListEnumerator.cs \
	Properties/AssemblyInfo.cs \
	Interfaces/Proxies/IGroupedCollectionProxy.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	app.config \
	dday.collections.pc.in 

REFERENCES =  \
	System \
	System.Core \
	System.Xml.Linq \
	System.Data.DataSetExtensions \
	System.Data \
	System.Xml

DLL_REFERENCES = 

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

DDAY_COLLECTIONS_DLL_CONFIG = $(BUILD_DIR)/DDay.Collections.dll.config
DDAY_COLLECTIONS_PC = $(BUILD_DIR)/dday.collections.pc

$(eval $(call emit-deploy-target,DDAY_COLLECTIONS_DLL_CONFIG))
$(eval $(call emit-deploy-wrapper,DDAY_COLLECTIONS_PC,dday.collections.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
