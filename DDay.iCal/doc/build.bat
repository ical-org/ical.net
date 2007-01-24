MRefBuilder "../bin/Debug/DDay.iCal.dll" /out:reflection.org
XslTransform "C:\Program Files\Sandcastle\ProductionTransforms\AddOverloads.xsl" reflection.org | XslTransform "C:\Program Files\Sandcastle\ProductionTransforms\AddGuidFilenames.xsl" /out:reflection.xml
XslTransform "C:\Program Files\Sandcastle\ProductionTransforms\ReflectionToManifest.xsl" reflection.xml /out:manifest.xml
if not exist Output mkdir Output
if not exist Output\html mkdir Output\html
if not exist Output\art mkdir Output\art
if not exist Output\scripts mkdir Output\scripts
if not exist Output\styles mkdir Output\styles
copy "C:\Program Files\Sandcastle\Presentation\Art\*" Output\art
copy "C:\Program Files\Sandcastle\Presentation\scripts\*" Output\scripts
copy "C:\Program Files\Sandcastle\Presentation\styles\*" Output\styles
BuildAssembler /config:sandcastle.config manifest.xml
XslTransform "C:\Program Files\Sandcastle\ProductionTransforms\ReflectionToChmContents.xsl" reflection.xml /arg:html=Output\html /out:test.hhc
copy test.hhc Output
copy "C:\Program Files\Sandcastle\Presentation\Chm\test.hhp" Output
cd Output
hhc test.hhp
ren Test.chm DDay.iCal.chm