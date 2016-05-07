
EXTRA_DIST =  

# Warning: This is an automatically generated file, do not edit!

if ENABLE_DEBUG
ASSEMBLY_COMPILER_COMMAND = mcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:TRACE;DEBUG;DATACONTRACT" 
ASSEMBLY = bin/Debug/DDay.iCal.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../DDay.Collections/DDay.Collections/bin/Debug/DDay.Collections.dll
BUILD_DIR = bin/Debug

DDAY_ICAL_DLL_MDB_SOURCE=bin/Debug/DDay.iCal.dll.mdb
DDAY_ICAL_DLL_MDB=$(BUILD_DIR)/DDay.iCal.dll.mdb
LICENSE_TXT_SOURCE=License.txt
ANTLR_RUNTIME_DLL_SOURCE=../antlr.runtime.dll
DDAY_COLLECTIONS_DLL_SOURCE=../DDay.Collections/DDay.Collections/bin/Debug/DDay.Collections.dll
DDAY_COLLECTIONS_DLL_CONFIG_SOURCE=../DDay.Collections/DDay.Collections/app.config

endif

if ENABLE_DEBUG_MIXED_PLATFORMS
ASSEMBLY_COMPILER_COMMAND = mcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:TRACE;DEBUG;DATACONTRACT" 
ASSEMBLY = bin/Debug/DDay.iCal.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../DDay.Collections/DDay.Collections/bin/Debug/DDay.Collections.dll
BUILD_DIR = bin/Debug

DDAY_ICAL_DLL_MDB_SOURCE=bin/Debug/DDay.iCal.dll.mdb
DDAY_ICAL_DLL_MDB=$(BUILD_DIR)/DDay.iCal.dll.mdb
LICENSE_TXT_SOURCE=License.txt
ANTLR_RUNTIME_DLL_SOURCE=../antlr.runtime.dll
DDAY_COLLECTIONS_DLL_SOURCE=../DDay.Collections/DDay.Collections/bin/Debug/DDay.Collections.dll
DDAY_COLLECTIONS_DLL_CONFIG_SOURCE=../DDay.Collections/DDay.Collections/app.config

endif

if ENABLE_DEBUG_X86
ASSEMBLY_COMPILER_COMMAND = mcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize- -debug "-define:TRACE;DEBUG;DATACONTRACT" 
ASSEMBLY = bin/Debug/DDay.iCal.dll
ASSEMBLY_MDB = $(ASSEMBLY).mdb
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../DDay.Collections/DDay.Collections/bin/Debug/DDay.Collections.dll
BUILD_DIR = bin/Debug

DDAY_ICAL_DLL_MDB_SOURCE=bin/Debug/DDay.iCal.dll.mdb
DDAY_ICAL_DLL_MDB=$(BUILD_DIR)/DDay.iCal.dll.mdb
LICENSE_TXT_SOURCE=License.txt
ANTLR_RUNTIME_DLL_SOURCE=../antlr.runtime.dll
DDAY_COLLECTIONS_DLL_SOURCE=../DDay.Collections/DDay.Collections/bin/Debug/DDay.Collections.dll
DDAY_COLLECTIONS_DLL_CONFIG_SOURCE=../DDay.Collections/DDay.Collections/app.config

endif

if ENABLE_RELEASE
ASSEMBLY_COMPILER_COMMAND = mcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ "-define:TRACE" 
ASSEMBLY = bin/Release/DDay.iCal.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../DDay.Collections/DDay.Collections/bin/Release/DDay.Collections.dll
BUILD_DIR = bin/Release

DDAY_ICAL_DLL_MDB=
LICENSE_TXT_SOURCE=License.txt
ANTLR_RUNTIME_DLL_SOURCE=../antlr.runtime.dll
DDAY_COLLECTIONS_DLL_SOURCE=../DDay.Collections/DDay.Collections/bin/Release/DDay.Collections.dll
DDAY_COLLECTIONS_DLL_CONFIG_SOURCE=../DDay.Collections/DDay.Collections/app.config

endif

if ENABLE_RELEASE_MIXED_PLATFORMS
ASSEMBLY_COMPILER_COMMAND = mcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ "-define:TRACE" 
ASSEMBLY = bin/Release/DDay.iCal.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../DDay.Collections/DDay.Collections/bin/Release/DDay.Collections.dll
BUILD_DIR = bin/Release

DDAY_ICAL_DLL_MDB=
LICENSE_TXT_SOURCE=License.txt
ANTLR_RUNTIME_DLL_SOURCE=../antlr.runtime.dll
DDAY_COLLECTIONS_DLL_SOURCE=../DDay.Collections/DDay.Collections/bin/Release/DDay.Collections.dll
DDAY_COLLECTIONS_DLL_CONFIG_SOURCE=../DDay.Collections/DDay.Collections/app.config

endif

if ENABLE_RELEASE_X86
ASSEMBLY_COMPILER_COMMAND = mcs
ASSEMBLY_COMPILER_FLAGS =  -noconfig -codepage:utf8 -warn:4 -optimize+ "-define:TRACE" 
ASSEMBLY = bin/Release/DDay.iCal.dll
ASSEMBLY_MDB = 
COMPILE_TARGET = library
PROJECT_REFERENCES =  \
	../DDay.Collections/DDay.Collections/bin/Release/DDay.Collections.dll
BUILD_DIR = bin/Release

DDAY_ICAL_DLL_MDB=
LICENSE_TXT_SOURCE=License.txt
ANTLR_RUNTIME_DLL_SOURCE=../antlr.runtime.dll
DDAY_COLLECTIONS_DLL_SOURCE=../DDay.Collections/DDay.Collections/bin/Release/DDay.Collections.dll
DDAY_COLLECTIONS_DLL_CONFIG_SOURCE=../DDay.Collections/DDay.Collections/app.config

endif

AL=al2
SATELLITE_ASSEMBLY_NAME=$(notdir $(basename $(ASSEMBLY))).resources.dll

PROGRAMFILES = \
	$(DDAY_ICAL_DLL_MDB) \
	$(LICENSE_TXT) \
	$(ANTLR_RUNTIME_DLL) \
	$(DDAY_COLLECTIONS_DLL) \
	$(DDAY_COLLECTIONS_DLL_CONFIG)  

LINUX_PKGCONFIG = \
	$(DDAY_ICAL_PC)  


RESGEN=resgen2
	
all: $(ASSEMBLY) $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

FILES = \
	Compatibility/BrowsableAttribute.cs \
	Compatibility/DebuggerDisplayAttribute.cs \
	Components/Alarm.cs \
	Components/FreeBusy.cs \
	Components/iCalTimeZone.cs \
	Components/iCalTimeZoneInfo.cs \
	Components/Journal.cs \
	DataTypes/Attachment.cs \
	Components/CalendarComponent.cs \
	Components/Event.cs \
	Components/RecurringComponent.cs \
	Components/Todo.cs \
	Components/UniqueComponent.cs \
	Constants.cs \
	DataTypes/Attendee.cs \
	DataTypes/CalendarDataType.cs \
	DataTypes/FreeBusyEntry.cs \
	DataTypes/IGroupedCollection.cs \
	DataTypes/Organizer.cs \
	DataTypes/RequestStatus.cs \
	DataTypes/StatusCode.cs \
	DataTypes/Trigger.cs \
	DataTypes/WeekDay.cs \
	DataTypes/EncodableDataType.cs \
	DataTypes/GeographicLocation.cs \
	DataTypes/PeriodList.cs \
	DataTypes/RecurrencePattern.cs \
	DataTypes/UTCOffset.cs \
	Evaluation/RecurrencePatternEvaluator.cs \
	Evaluation/TimeZoneEvaluator.cs \
	Evaluation/EventEvaluator.cs \
	Evaluation/Evaluator.cs \
	Evaluation/PeriodListEvaluator.cs \
	Evaluation/RecurringEvaluator.cs \
	Evaluation/TimeZoneInfoEvaluator.cs \
	Evaluation/TodoEvaluator.cs \
	Exceptions/EvaluationEngineException.cs \
	ExtensionMethods/CalendarObjectExtensions.cs \
	ExtensionMethods/ListExtensions.cs \
	General/CalendarObjectList.cs \
	General/Proxies/CalendarObjectListProxy.cs \
	General/Proxies/CalendarParameterCollectionProxy.cs \
	General/ServiceProvider.cs \
	General/Proxies/UniqueComponentListProxy.cs \
	Interfaces/DataTypes/IFreeBusyEntry.cs \
	Interfaces/Evaluation/IGetFreeBusy.cs \
	Interfaces/Evaluation/IGetOccurrences.cs \
	Interfaces/Components/IAlarmContainer.cs \
	Interfaces/DataTypes/ICalendarDataType.cs \
	Interfaces/DataTypes/IDateTime.cs \
	Interfaces/DataTypes/IWeekDay.cs \
	Interfaces/DataTypes/IEncodableDataType.cs \
	Interfaces/DataTypes/IGeographicLocation.cs \
	Interfaces/DataTypes/IPeriod.cs \
	Interfaces/Evaluation/IEvaluator.cs \
	Interfaces/Evaluation/IRecurrable.cs \
	Interfaces/General/ICalendarObjectList.cs \
	Interfaces/General/IUniqueComponentList.cs \
	Interfaces/General/Proxies/ICalendarParameterCollectionProxy.cs \
	Interfaces/General/IServiceProvider.cs \
	Interfaces/Serialization/Factory/ICalendarComponentFactory.cs \
	Interfaces/Serialization/Factory/ISerializerFactory.cs \
	Interfaces/Serialization/IDataTypeMapper.cs \
	Interfaces/Serialization/IEncodingProvider.cs \
	Interfaces/Serialization/IEncodingStack.cs \
	Interfaces/Serialization/ISerializationContext.cs \
	Interfaces/Serialization/ISerializationProcessor.cs \
	Interfaces/Serialization/ISerializationSettings.cs \
	Interfaces/Serialization/ISerializer.cs \
	Interfaces/Serialization/IStringSerializer.cs \
	Serialization/EncodingStack.cs \
	Serialization/iCalendar/Factory/DataTypeSerializerFactory.cs \
	Serialization/iCalendar/Processors/ComponentPropertyConsolidator.cs \
	Serialization/iCalendar/Processors/CompositeProcessor.cs \
	Serialization/iCalendar/SerializationInfo.cs \
	Serialization/iCalendar/Serializers/Components/EventSerializer.cs \
	Serialization/iCalendar/Serializers/DataTypes/AttachmentSerializer.cs \
	Serialization/iCalendar/Serializers/DataTypes/AttendeeSerializer.cs \
	Serialization/iCalendar/Serializers/DataTypes/FreeBusyEntrySerializer.cs \
	Serialization/iCalendar/Serializers/DataTypes/OrganizerSerializer.cs \
	Serialization/iCalendar/Serializers/Other/UriSerializer.cs \
	Serialization/iCalendar/Serializers/Other/EnumSerializer.cs \
	Serialization/iCalendar/Serializers/DataTypes/TriggerSerializer.cs \
	Serialization/iCalendar/Serializers/DataTypes/StatusCodeSerializer.cs \
	Serialization/iCalendar/Serializers/DataTypes/RequestStatusSerializer.cs \
	Serialization/iCalendar/Serializers/DataTypes/PeriodSerializer.cs \
	Serialization/iCalendar/Serializers/DataTypes/PeriodListSerializer.cs \
	Serialization/iCalendar/Serializers/DataTypes/DataTypeSerializer.cs \
	Serialization/iCalendar/Serializers/DataTypes/WeekDaySerializer.cs \
	Serialization/iCalendar/Serializers/DataTypes/EncodableDataTypeSerializer.cs \
	Serialization/iCalendar/Serializers/DataTypes/GeographicLocationSerializer.cs \
	Serialization/iCalendar/Serializers/Other/IntegerSerializer.cs \
	Serialization/iCalendar/Serializers/DataTypes/RecurrencePatternSerializer.cs \
	Serialization/iCalendar/Serializers/Other/TimeSpanSerializer.cs \
	Serialization/iCalendar/Serializers/DataTypes/UTCOffsetSerializer.cs \
	Serialization/iCalendar/Serializers/GenericListSerializer.cs \
	Serialization/iCalendar/Serializers/Other/StringSerializer.cs \
	Serialization/EncodingProvider.cs \
	Structs/AlarmOccurrence.cs \
	Structs/Occurrence.cs \
	DataTypes/Period.cs \
	Serialization/Factory/ComponentFactory.cs \
	General/CalendarObject.cs \
	General/CalendarObjectBase.cs \
	General/CalendarParameter.cs \
	General/CalendarParameterList.cs \
	General/CalendarProperty.cs \
	General/CalendarPropertyList.cs \
	DataTypes/iCalDateTime.cs \
	Factory/UIDFactory.cs \
	iCalendar.cs \
	iCalendarCollection.cs \
	Interfaces/Components/IAlarm.cs \
	Interfaces/Components/IEvent.cs \
	Interfaces/Components/IFreeBusy.cs \
	Interfaces/Components/IJournal.cs \
	Interfaces/Components/ITimeZone.cs \
	Interfaces/Components/ITodo.cs \
	Interfaces/DataTypes/IAttendee.cs \
	Interfaces/DataTypes/IAttachment.cs \
	Interfaces/DataTypes/IOrganizer.cs \
	Interfaces/DataTypes/IPeriodList.cs \
	Interfaces/DataTypes/IRecurrencePattern.cs \
	Interfaces/DataTypes/IRequestStatus.cs \
	Interfaces/DataTypes/IStatusCode.cs \
	Interfaces/Components/ITimeZoneInfo.cs \
	Interfaces/DataTypes/ITrigger.cs \
	Interfaces/DataTypes/IUTCOffset.cs \
	Interfaces/IICalendarCollection.cs \
	Interfaces/Components/IRecurringComponent.cs \
	Interfaces/Components/IUniqueComponent.cs \
	Interfaces/Components/ICalendarComponent.cs \
	Interfaces/General/ICalendarPropertyList.cs \
	Interfaces/General/ICalendarObject.cs \
	Interfaces/General/ICalendarParameter.cs \
	Interfaces/General/ICalendarParameterCollection.cs \
	Interfaces/General/ICalendarParameterCollectionContainer.cs \
	Interfaces/General/ICalendarProperty.cs \
	Interfaces/General/ICalendarPropertyListContainer.cs \
	Interfaces/General/ICopyable.cs \
	Interfaces/General/ILoadable.cs \
	Interfaces/General/IMergeable.cs \
	Interfaces/IICalendar.cs \
	Properties/AssemblyInfo.cs \
	Serialization/DataTypeMapper.cs \
	Serialization/iCalendar/Serializers/DataMapSerializer.cs \
	Serialization/iCalendar/Serializers/DataTypes/DateTimeSerializer.cs \
	Serialization/iCalendar/Serializers/ParameterSerializer.cs \
	Serialization/iCalendar/Serializers/iCalendarSerializer.cs \
	Serialization/iCalendar/Serializers/Components/ComponentSerializer.cs \
	Serialization/iCalendar/Serializers/PropertySerializer.cs \
	Serialization/iCalendar/Serializers/SerializerBase.cs \
	Serialization/iCalendar/Factory/SerializerFactory.cs \
	Serialization/iCalendar/iCalLexer.cs \
	Serialization/iCalendar/iCalParser.cs \
	Serialization/iCalendar/iCalParserTokenTypes.cs \
	Serialization/SerializationContext.cs \
	Serialization/SerializationSettings.cs \
	Structs/TimeZoneObservance.cs \
	Utility/AssociationUtil.cs \
	Utility/DateUtil.cs \
	Utility/KnownTypeHelper.cs \
	Utility/RecurrenceUtil.cs \
	Utility/SerializationUtil.cs \
	Utility/TextUtil.cs 

DATA_FILES = 

RESOURCES = 

EXTRAS = \
	Properties/AssemblyInfo.cs.template \
	Serialization/iCalendar/iCal.g \
	License.txt \
	Serialization/iCalendar/iCalParserTokenTypes.txt \
	 \
	dday.ical.pc.in 

REFERENCES =  \
	System \
	System.Core \
	System.Data \
	System.Drawing \
	System.Runtime.Serialization \
	System.Windows.Forms \
	System.Xml

DLL_REFERENCES =  \
	../antlr.runtime.dll

CLEANFILES = $(PROGRAMFILES) $(LINUX_PKGCONFIG) 

include $(top_srcdir)/Makefile.include

LICENSE_TXT = $(BUILD_DIR)/License.txt
ANTLR_RUNTIME_DLL = $(BUILD_DIR)/antlr.runtime.dll
DDAY_COLLECTIONS_DLL = $(BUILD_DIR)/DDay.Collections.dll
DDAY_COLLECTIONS_DLL_CONFIG = $(BUILD_DIR)/DDay.Collections.dll.config
DDAY_ICAL_PC = $(BUILD_DIR)/dday.ical.pc

$(eval $(call emit-deploy-target,LICENSE_TXT))
$(eval $(call emit-deploy-target,ANTLR_RUNTIME_DLL))
$(eval $(call emit-deploy-target,DDAY_COLLECTIONS_DLL))
$(eval $(call emit-deploy-target,DDAY_COLLECTIONS_DLL_CONFIG))
$(eval $(call emit-deploy-wrapper,DDAY_ICAL_PC,dday.ical.pc))


$(eval $(call emit_resgen_targets))
$(build_xamlg_list): %.xaml.g.cs: %.xaml
	xamlg '$<'

$(ASSEMBLY_MDB): $(ASSEMBLY)

$(ASSEMBLY): $(build_sources) $(build_resources) $(build_datafiles) $(DLL_REFERENCES) $(PROJECT_REFERENCES) $(build_xamlg_list) $(build_satellite_assembly_list)
	mkdir -p $(shell dirname $(ASSEMBLY))
	$(ASSEMBLY_COMPILER_COMMAND) $(ASSEMBLY_COMPILER_FLAGS) -out:$(ASSEMBLY) -target:$(COMPILE_TARGET) $(build_sources_embed) $(build_resources_embed) $(build_references_ref)
