//
// Copyright ical.net project maintainers and contributors.
// Licensed under the MIT license.
//

using System.Collections.Generic;
using System.IO;
using System.Linq;
using BenchmarkDotNet.Attributes;
using Ical.Net.Serialization;
using Ical.Net.Serialization.DataTypes;

namespace Ical.Net.Benchmarks;

[MemoryDiagnoser]
public class StringSerializerBenchmarks
{


    private static readonly string ShortEscape = "Two\\\\Words";

    private static readonly string MediumEscape = """
        Project xyz Review Meeting Minutes\n
         Agenda\n1. Review of project version 1.0 requirements.\n2.
         Definition of project processes.\n3. Review of project schedule.\n
         Participants: John Smith, Jane Doe, Jim Dandy\n-It was
          decided that the requirements need to be signed off by
          product marketing.\n-Project processes were accepted.\n
         -Project schedule needs to account for scheduled holidays
          and employee vacation time. Check with HR for specific
          dates.\n-New schedule will be distributed by Friday.\n-
         Next weeks meeting is cancelled. No meeting until 3/23.
        """;

    private static readonly string LongEscape = """
        <p>Lorem ipsum dolor sit amet\, consectetur adipiscing elit. Nu
         lla vel lectus libero. Lorem ipsum dolor sit amet\, consectetur adipiscing 
         elit. Duis in quam blandit\, mollis ante eu\, consectetur turpis. Aenean nu
         lla justo\, feugiat eu aliquet ac\, elementum vel diam. Pellentesque laoree
         t odio ut sollicitudin lobortis. Vestibulum nisl lorem\, dignissim in liber
         o a\, elementum suscipit tortor. Duis mi eros\, cursus in leo in\, consequa
         t pulvinar sem. Morbi semper nibh in fringilla luctus.</p><p>Aliquam vel du
         i dignissim\, rutrum enim eget\, faucibus libero. Maecenas pretium feugiat 
         ligula\, quis vehicula sem. Vivamus eget ultricies mauris\, at ullamcorper 
         justo. Praesent porttitor vel nibh in tempus. Aliquam cursus erat eu libero
          ultrices\, ac finibus tellus lacinia. Cras venenatis diam ut lacus blandit
         \, id condimentum purus dictum. Nam iaculis eros ultrices commodo posuere. 
         Pellentesque bibendum lobortis pharetra. Sed varius enim vel laoreet interd
         um.</p><p>Suspendisse lectus ex\, pharetra ac mi in\, viverra mattis turpis
         . Donec maximus sapien quis posuere lacinia. Quisque vel quam purus. Aenean
          scelerisque erat a ullamcorper iaculis. Fusce sollicitudin nulla orci\, vi
         tae tempus nulla dapibus vel. Aliquam id purus dignissim\, molestie libero 
         sed\, dapibus sapien. Vestibulum ante ipsum primis in faucibus orci luctus 
         et ultrices posuere cubilia curae\; Suspendisse sodales ultricies eros\, eu
          eleifend nisl placerat ac. Aliquam sit amet lacus quam.</p><p>Pellentesque
          ornare\, orci vitae porta accumsan\, dolor magna consequat ipsum\, vel pre
         tium libero nunc vel felis. Sed et purus eget libero tristique fermentum eg
         et a turpis. Duis tempor vehicula enim et condimentum. Aliquam viverra ante
          quis varius blandit. Donec in quam arcu. Cras sit amet libero placerat\, m
         olestie eros id\, dignissim justo. Fusce ut lorem tortor.</p><p>Curabitur e
         t viverra risus\, ac semper erat. Nullam nulla nisl\, fringilla vel quam no
         n\, sagittis venenatis est. Nulla suscipit commodo nulla\, et aliquet mauri
         s pellentesque quis. Aenean euismod cursus velit\, vel scelerisque ipsum eu
         ismod nec. Maecenas id feugiat purus. Aenean elit ligula\, vestibulum in eg
         estas in\, dictum in nisl. Aliquam nisi justo\, aliquet sed gravida in\, mo
         llis non nisl. Etiam pellentesque gravida quam rhoncus porta. Vestibulum ul
         lamcorper odio sed elit placerat\, a mollis dolor viverra. Proin scelerisqu
         e\, ante vitae pellentesque luctus\, neque est gravida enim\, in faucibus e
         x urna vitae erat. Quisque ut mi sit amet lectus laoreet efficitur. Vivamus
          non diam nibh. Nulla lectus enim\, sodales id finibus ut\, faucibus ut vel
         it. Vivamus sollicitudin ex sed leo cursus posuere. Duis efficitur dolor du
         i\, sed gravida nunc gravida ut.</p><p>Aenean ut sapien id ipsum venenatis 
         lacinia. Donec aliquam fermentum diam\, vel molestie ante suscipit eget. Pe
         llentesque ut eleifend sapien\, sit amet pretium tortor. Nam sollicitudin c
         ursus libero non mollis. Vivamus elementum lobortis sem sed ornare. Nunc to
         rtor nisi\, tempor et tempus venenatis\, mattis et nibh. Nulla facilisi. In
          sed ultrices est.</p><p>Phasellus non tincidunt dui\, fringilla egestas au
         gue. Fusce consectetur dictum metus\, et efficitur odio dapibus ac. Vestibu
         lum tristique est non odio porttitor porttitor. Aenean imperdiet ultricies 
         mollis. Integer blandit suscipit urna at aliquam. Orci varius natoque penat
         ibus et magnis dis parturient montes\, nascetur ridiculus mus. Morbi gravid
         a nunc neque\, eget semper felis facilisis et. Curabitur arcu sapien\, vehi
         cula id ante id\, ultrices finibus neque. Sed purus metus\, pellentesque se
         d sapien ut\, luctus finibus odio. Phasellus elit ligula\, vulputate eget n
         ulla eget\, imperdiet aliquet nisi. Nullam vel mi id purus condimentum accu
         msan quis sed lorem. Vestibulum varius nunc rutrum euismod varius. Sed just
         o mauris\, egestas quis tellus non\, iaculis aliquet dui. Praesent malesuad
         a eu erat id auctor. Etiam eget libero diam. Integer sed hendrerit nisi\, m
         ollis vehicula libero.</p><p>Integer iaculis id felis at porta. Proin pelle
         ntesque ornare sodales. Fusce scelerisque eleifend facilisis. Morbi ultrici
         es lorem lorem\, ac molestie erat placerat in. Quisque eget blandit elit. A
         enean tortor orci\, vestibulum id vehicula id\, bibendum sed diam. Vestibul
         um vehicula augue ac eleifend pulvinar. Nulla rutrum nulla ut erat pulvinar
         \, non eleifend dolor elementum. Vivamus vulputate eros in ante pharetra\, 
         vitae feugiat ligula faucibus. In aliquet suscipit sapien et commodo. Nulla
         m hendrerit placerat nulla\, ut rhoncus turpis luctus quis.</p><p>Fusce tor
         tor eros\, aliquam ut lacinia vel\, suscipit eget magna. Pellentesque imper
         diet convallis justo. Ut sit amet velit ut lacus auctor tempor. Aenean lobo
         rtis dui nulla\, at consequat odio pulvinar id. Sed ut feugiat odio\, et mo
         lestie ipsum. Morbi a orci iaculis\, dignissim sem volutpat\, dictum sapien
         . Etiam ut dolor blandit\, congue turpis eu\, accumsan purus. Morbi viverra
          bibendum dui\, a dapibus sem aliquam vitae. Aliquam sit amet consectetur s
         em\, sed cursus lorem. Aliquam et nisi vulputate\, placerat lorem in\, euis
         mod lectus. Proin tempor tincidunt gravida. Maecenas a feugiat nulla. Donec
          tristique dolor et tellus sollicitudin\, ut malesuada odio rutrum. Mauris 
         rutrum dolor libero\, id ullamcorper nibh vulputate vel. Nam imperdiet augu
         e ut dui interdum\, sed sollicitudin felis lacinia. Fusce dignissim risus e
         nim\, ac maximus leo tincidunt a.</p><p>Quisque mattis tellus sed hendrerit
          molestie. Ut varius erat nunc\, non maximus sem malesuada sit amet. Vestib
         ulum id massa eu eros mollis consectetur at in ex. Vestibulum ultricies id 
         sem a molestie. Morbi ornare magna a sagittis pulvinar. Donec nec quam at o
         rci dictum dapibus tincidunt vulputate ligula. Etiam diam nibh\, facilisis 
         eget placerat et\, pellentesque sit amet nisi. Integer in nunc elementum\, 
         pellentesque justo eu\, pretium elit. Aliquam suscipit dapibus ornare. Prae
         sent nec sem mi. Nunc cursus\, justo nec dignissim volutpat\, dui mauris cu
         rsus magna\, sed maximus nisi ligula id nunc. Ut non neque sed nisi placera
         t fringilla.</p><p>Phasellus commodo finibus semper. In hac habitasse plate
         a dictumst. Duis non diam euismod\, sodales sem congue\, porttitor nunc. Ut
          at dapibus orci. Curabitur euismod tincidunt eleifend. Nullam diam nibh\, 
         rutrum id dolor non\, cursus dapibus leo. Aenean sed purus in elit eleifend
          finibus. Mauris luctus fringilla eleifend. Duis et dui orci. In interdum i
         nterdum enim\, eget condimentum odio porta aliquet. Maecenas non nisl mauri
         s. Sed neque libero\, venenatis id dolor nec\, rhoncus egestas ligula. In n
         on felis suscipit\, cursus risus nec\, volutpat enim.</p><p>Sed iaculis feu
         giat risus ac tempor. Quisque nec orci elit. Nullam hendrerit tellus velit\
         , et semper leo bibendum ac. Cras sagittis\, metus in vehicula fringilla\, 
         sem mauris sodales magna\, vel sagittis nisi magna quis odio. Donec arcu le
         o\, fringilla quis erat a\, rutrum eleifend diam. Ut ligula mi\, sagittis u
         t porttitor non\, pulvinar a ipsum. Maecenas non lectus vel nisl convallis 
         euismod. Vestibulum egestas tincidunt convallis. Mauris nec leo in felis sa
         gittis dapibus. Fusce scelerisque pretium blandit. Sed pretium varius sapie
         n quis vulputate. Vestibulum aliquet ultrices euismod.</p><p>Duis neque ero
         s\, ornare a risus nec\, luctus mattis dolor. Proin eget consectetur nisl. 
         Donec a quam sit amet dui gravida lobortis. Aenean quis tellus iaculis\, la
         cinia lacus nec\, eleifend ligula. Curabitur malesuada finibus enim id eges
         tas. Nullam leo magna\, blandit non tempor in\, laoreet ut mauris. Sed rutr
         um pellentesque justo. Integer varius malesuada urna a facilisis. Donec bib
         endum maximus sapien. Morbi rhoncus rutrum massa id posuere.</p><p>Duis max
         imus eu ex in cursus. Mauris dictum porttitor augue. Nam sed tortor lorem. 
         Cras convallis odio odio\, dignissim viverra sapien ultrices ut. Nunc pelle
         ntesque lacinia eros\, id lobortis nulla dapibus quis. Phasellus ac eleifen
         d ante. Nunc massa justo\, viverra non commodo at\, finibus eget augue. Pel
         lentesque tincidunt\, nisi eget molestie viverra\, odio orci semper ligula\
         , ornare pellentesque sem sem vulputate libero. Quisque tincidunt sapien qu
         is enim tincidunt porttitor. Maecenas pulvinar eros.</p>
        """;

    private static readonly string ShortNoEscape = "19970324T120000Z";

    private static readonly string LongNoEscape = """
        Lorem ipsum dolor sit amet consectetur adipiscing elit. Nulla v
         el lectus libero. Lorem ipsum dolor sit amet consectetur adipiscing elit. D
         uis in quam blandit mollis ante eu consectetur turpis. Aenean nulla justo f
         eugiat eu aliquet ac elementum vel diam. Pellentesque laoreet odio ut solli
         citudin lobortis. Vestibulum nisl lorem dignissim in libero a elementum sus
         cipit tortor. Duis mi eros cursus in leo in consequat pulvinar sem. Morbi s
         emper nibh in fringilla luctus. Aliquam vel dui dignissim rutrum enim eget 
         faucibus libero. Maecenas pretium feugiat ligula quis vehicula sem. Vivamus
          eget ultricies mauris at ullamcorper justo. Praesent porttitor vel nibh in
          tempus. Aliquam cursus erat eu libero ultrices ac finibus tellus lacinia. 
         Cras venenatis diam ut lacus blandit id condimentum purus dictum. Nam iacul
         is eros ultrices commodo posuere. Pellentesque bibendum lobortis pharetra. 
         Sed varius enim vel laoreet interdum. Suspendisse lectus ex pharetra ac mi 
         in viverra mattis turpis. Donec maximus sapien quis posuere lacinia. Quisqu
         e vel quam purus. Aenean scelerisque erat a ullamcorper iaculis. Fusce soll
         icitudin nulla orci vitae tempus nulla dapibus vel. Aliquam id purus dignis
         sim molestie libero sed dapibus sapien. Vestibulum ante ipsum primis in fau
         cibus orci luctus et ultrices posuere cubilia curae Suspendisse sodales ult
         ricies eros eu eleifend nisl placerat ac. Aliquam sit amet lacus quam. Pell
         entesque ornare orci vitae porta accumsan dolor magna consequat ipsum vel p
         retium libero nunc vel felis. Sed et purus eget libero tristique fermentum 
         eget a turpis. Duis tempor vehicula enim et condimentum. Aliquam viverra an
         te quis varius blandit. Donec in quam arcu. Cras sit amet libero placerat m
         olestie eros id dignissim justo. Fusce ut lorem tortor. Curabitur et viverr
         a risus ac semper erat. Nullam nulla nisl fringilla vel quam non sagittis v
         enenatis est. Nulla suscipit commodo nulla et aliquet mauris pellentesque q
         uis. Aenean euismod cursus velit vel scelerisque ipsum euismod nec. Maecena
         s id feugiat purus. Aenean elit ligula vestibulum in egestas in dictum in n
         isl. Aliquam nisi justo aliquet sed gravida in mollis non nisl. Etiam pelle
         ntesque gravida quam rhoncus porta. Vestibulum ullamcorper odio sed elit pl
         acerat a mollis dolor viverra. Proin scelerisque ante vitae pellentesque lu
         ctus neque est gravida enim in faucibus ex urna vitae erat. Quisque ut mi s
         it amet lectus laoreet efficitur. Vivamus non diam nibh. Nulla lectus enim 
         sodales id finibus ut faucibus ut velit. Vivamus sollicitudin ex sed leo cu
         rsus posuere. Duis efficitur dolor dui sed gravida nunc gravida ut. Aenean 
         ut sapien id ipsum venenatis lacinia. Donec aliquam fermentum diam vel mole
         stie ante suscipit eget. Pellentesque ut eleifend sapien sit amet pretium t
         ortor. Nam sollicitudin cursus libero non mollis. Vivamus elementum loborti
         s sem sed ornare. Nunc tortor nisi tempor et tempus venenatis mattis et nib
         h. Nulla facilisi. In sed ultrices est. Phasellus non tincidunt dui fringil
         la egestas augue. Fusce consectetur dictum metus et efficitur odio dapibus 
         ac. Vestibulum tristique est non odio porttitor porttitor. Aenean imperdiet
          ultricies mollis. Integer blandit suscipit urna at aliquam. Orci varius na
         toque penatibus et magnis dis parturient montes nascetur ridiculus mus. Mor
         bi gravida nunc neque eget semper felis facilisis et. Curabitur arcu sapien
          vehicula id ante id ultrices finibus neque. Sed purus metus pellentesque s
         ed sapien ut luctus finibus odio. Phasellus elit ligula vulputate eget null
         a eget imperdiet aliquet nisi. Nullam vel mi id purus condimentum accumsan 
         quis sed lorem. Vestibulum varius nunc rutrum euismod varius. Sed justo mau
         ris egestas quis tellus non iaculis aliquet dui. Praesent malesuada eu erat
          id auctor. Etiam eget libero diam. Integer sed hendrerit nisi mollis vehic
         ula libero. Integer iaculis id felis at porta. Proin pellentesque ornare so
         dales. Fusce scelerisque eleifend facilisis. Morbi ultricies lorem lorem ac
          molestie erat placerat in. Quisque eget blandit elit. Aenean tortor orci v
         estibulum id vehicula id bibendum sed diam. Vestibulum vehicula augue ac el
         eifend pulvinar. Nulla rutrum nulla ut erat pulvinar non eleifend dolor ele
         mentum. Vivamus vulputate eros in ante pharetra vitae feugiat ligula faucib
         us. In aliquet suscipit sapien et commodo. Nullam hendrerit placerat nulla 
         ut rhoncus turpis luctus quis. Fusce tortor eros aliquam ut lacinia vel sus
         cipit eget magna. Pellentesque imperdiet convallis justo. Ut sit amet velit
          ut lacus auctor tempor. Aenean lobortis dui nulla at consequat odio pulvin
         ar id. Sed ut feugiat odio et molestie ipsum. Morbi a orci iaculis dignissi
         m sem volutpat dictum sapien. Etiam ut dolor blandit congue turpis eu accum
         san purus. Morbi viverra bibendum dui a dapibus sem aliquam vitae. Aliquam 
         sit amet consectetur sem sed cursus lorem. Aliquam et nisi vulputate placer
         at lorem in euismod lectus. Proin tempor tincidunt gravida. Maecenas a feug
         iat nulla. Donec tristique dolor et tellus sollicitudin ut malesuada odio r
         utrum. Mauris rutrum dolor libero id ullamcorper nibh vulputate vel. Nam im
         perdiet augue ut dui interdum sed sollicitudin felis lacinia. Fusce digniss
         im risus enim ac maximus leo tincidunt a. Quisque mattis tellus sed hendrer
         it molestie. Ut varius erat nunc non maximus sem malesuada sit amet. Vestib
         ulum id massa eu eros mollis consectetur at in ex. Vestibulum ultricies id 
         sem a molestie. Morbi ornare magna a sagittis pulvinar. Donec nec quam at o
         rci dictum dapibus tincidunt vulputate ligula. Etiam diam nibh facilisis eg
         et placerat et pellentesque sit amet nisi. Integer in nunc elementum pellen
         tesque justo eu pretium elit. Aliquam suscipit dapibus ornare. Praesent nec
          sem mi. Nunc cursus justo nec dignissim volutpat dui mauris cursus magna s
         ed maximus nisi ligula id nunc. Ut non neque sed nisi placerat fringilla. P
         hasellus commodo finibus semper. In hac habitasse platea dictumst. Duis non
          diam euismod sodales sem congue porttitor nunc. Ut at dapibus orci. Curabi
         tur euismod tincidunt eleifend. Nullam diam nibh rutrum id dolor non cursus
          dapibus leo. Aenean sed purus in elit eleifend finibus. Mauris luctus frin
         gilla eleifend. Duis et dui orci. In interdum interdum enim eget condimentu
         m odio porta aliquet. Maecenas non nisl mauris. Sed neque libero venenatis 
         id dolor nec rhoncus egestas ligula. In non felis suscipit cursus risus nec
          volutpat enim. Sed iaculis feugiat risus ac tempor. Quisque nec orci elit.
          Nullam hendrerit tellus velit et semper leo bibendum ac. Cras sagittis met
         us in vehicula fringilla sem mauris sodales magna vel sagittis nisi magna q
         uis odio. Donec arcu leo fringilla quis erat a rutrum eleifend diam. Ut lig
         ula mi sagittis ut porttitor non pulvinar a ipsum. Maecenas non lectus vel 
         nisl convallis euismod. Vestibulum egestas tincidunt convallis. Mauris nec 
         leo in felis sagittis dapibus. Fusce scelerisque pretium blandit. Sed preti
         um varius sapien quis vulputate. Vestibulum aliquet ultrices euismod. Duis 
         neque eros ornare a risus nec luctus mattis dolor. Proin eget consectetur n
         isl. Donec a quam sit amet dui gravida lobortis. Aenean quis tellus iaculis
          lacinia lacus nec eleifend ligula. Curabitur malesuada finibus enim id ege
         stas. Nullam leo magna blandit non tempor in laoreet ut mauris. Sed rutrum 
         pellentesque justo. Integer varius malesuada urna a facilisis. Donec bibend
         um maximus sapien. Morbi rhoncus rutrum massa id posuere. Duis maximus eu e
         x in cursus. Mauris dictum porttitor augue. Nam sed tortor lorem. Cras conv
         allis odio odio dignissim viverra sapien ultrices ut. Nunc pellentesque lac
         inia eros id lobortis nulla dapibus quis. Phasellus ac eleifend ante. Nunc 
         massa justo viverra non commodo at finibus eget augue. Pellentesque tincidu
         nt nisi eget molestie viverra odio orci semper ligula ornare pellentesque s
         em sem vulputate libero. Quisque tincidunt sapien quis enim tincidunt portt
         itor. Maecenas pulvinar eros.
        """;

    private static readonly string EventWithLongDescription = """
        BEGIN:VCALENDAR
        PRODID:-//Google Inc//Google Calendar 70.9054//EN
        VERSION:2.0
        METHOD:PUBLISH
        BEGIN:VEVENT
        DTSTART;VALUE=DATE:20260722
        DTEND;VALUE=DATE:20260723
        DTSTAMP:20260720T233352Z
        UID:12345
        CREATED:20260720T232805Z
        DESCRIPTION:Lorem ipsum dolor sit amet consectetur adipiscing elit. Nulla v
         el lectus libero. Lorem ipsum dolor sit amet consectetur adipiscing elit. D
         uis in quam blandit mollis ante eu consectetur turpis. Aenean nulla justo f
         eugiat eu aliquet ac elementum vel diam. Pellentesque laoreet odio ut solli
         citudin lobortis. Vestibulum nisl lorem dignissim in libero a elementum sus
         cipit tortor. Duis mi eros cursus in leo in consequat pulvinar sem. Morbi s
         emper nibh in fringilla luctus. Aliquam vel dui dignissim rutrum enim eget 
         faucibus libero. Maecenas pretium feugiat ligula quis vehicula sem. Vivamus
          eget ultricies mauris at ullamcorper justo. Praesent porttitor vel nibh in
          tempus. Aliquam cursus erat eu libero ultrices ac finibus tellus lacinia. 
         Cras venenatis diam ut lacus blandit id condimentum purus dictum. Nam iacul
         is eros ultrices commodo posuere. Pellentesque bibendum lobortis pharetra. 
         Sed varius enim vel laoreet interdum. Suspendisse lectus ex pharetra ac mi 
         in viverra mattis turpis. Donec maximus sapien quis posuere lacinia. Quisqu
         e vel quam purus. Aenean scelerisque erat a ullamcorper iaculis. Fusce soll
         icitudin nulla orci vitae tempus nulla dapibus vel. Aliquam id purus dignis
         sim molestie libero sed dapibus sapien. Vestibulum ante ipsum primis in fau
         cibus orci luctus et ultrices posuere cubilia curae Suspendisse sodales ult
         ricies eros eu eleifend nisl placerat ac. Aliquam sit amet lacus quam. Pell
         entesque ornare orci vitae porta accumsan dolor magna consequat ipsum vel p
         retium libero nunc vel felis. Sed et purus eget libero tristique fermentum 
         eget a turpis. Duis tempor vehicula enim et condimentum. Aliquam viverra an
         te quis varius blandit. Donec in quam arcu. Cras sit amet libero placerat m
         olestie eros id dignissim justo. Fusce ut lorem tortor. Curabitur et viverr
         a risus ac semper erat. Nullam nulla nisl fringilla vel quam non sagittis v
         enenatis est. Nulla suscipit commodo nulla et aliquet mauris pellentesque q
         uis. Aenean euismod cursus velit vel scelerisque ipsum euismod nec. Maecena
         s id feugiat purus. Aenean elit ligula vestibulum in egestas in dictum in n
         isl. Aliquam nisi justo aliquet sed gravida in mollis non nisl. Etiam pelle
         ntesque gravida quam rhoncus porta. Vestibulum ullamcorper odio sed elit pl
         acerat a mollis dolor viverra. Proin scelerisque ante vitae pellentesque lu
         ctus neque est gravida enim in faucibus ex urna vitae erat. Quisque ut mi s
         it amet lectus laoreet efficitur. Vivamus non diam nibh. Nulla lectus enim 
         sodales id finibus ut faucibus ut velit. Vivamus sollicitudin ex sed leo cu
         rsus posuere. Duis efficitur dolor dui sed gravida nunc gravida ut. Aenean 
         ut sapien id ipsum venenatis lacinia. Donec aliquam fermentum diam vel mole
         stie ante suscipit eget. Pellentesque ut eleifend sapien sit amet pretium t
         ortor. Nam sollicitudin cursus libero non mollis. Vivamus elementum loborti
         s sem sed ornare. Nunc tortor nisi tempor et tempus venenatis mattis et nib
         h. Nulla facilisi. In sed ultrices est. Phasellus non tincidunt dui fringil
         la egestas augue. Fusce consectetur dictum metus et efficitur odio dapibus 
         ac. Vestibulum tristique est non odio porttitor porttitor. Aenean imperdiet
          ultricies mollis. Integer blandit suscipit urna at aliquam. Orci varius na
         toque penatibus et magnis dis parturient montes nascetur ridiculus mus. Mor
         bi gravida nunc neque eget semper felis facilisis et. Curabitur arcu sapien
          vehicula id ante id ultrices finibus neque. Sed purus metus pellentesque s
         ed sapien ut luctus finibus odio. Phasellus elit ligula vulputate eget null
         a eget imperdiet aliquet nisi. Nullam vel mi id purus condimentum accumsan 
         quis sed lorem. Vestibulum varius nunc rutrum euismod varius. Sed justo mau
         ris egestas quis tellus non iaculis aliquet dui. Praesent malesuada eu erat
          id auctor. Etiam eget libero diam. Integer sed hendrerit nisi mollis vehic
         ula libero. Integer iaculis id felis at porta. Proin pellentesque ornare so
         dales. Fusce scelerisque eleifend facilisis. Morbi ultricies lorem lorem ac
          molestie erat placerat in. Quisque eget blandit elit. Aenean tortor orci v
         estibulum id vehicula id bibendum sed diam. Vestibulum vehicula augue ac el
         eifend pulvinar. Nulla rutrum nulla ut erat pulvinar non eleifend dolor ele
         mentum. Vivamus vulputate eros in ante pharetra vitae feugiat ligula faucib
         us. In aliquet suscipit sapien et commodo. Nullam hendrerit placerat nulla 
         ut rhoncus turpis luctus quis. Fusce tortor eros aliquam ut lacinia vel sus
         cipit eget magna. Pellentesque imperdiet convallis justo. Ut sit amet velit
          ut lacus auctor tempor. Aenean lobortis dui nulla at consequat odio pulvin
         ar id. Sed ut feugiat odio et molestie ipsum. Morbi a orci iaculis dignissi
         m sem volutpat dictum sapien. Etiam ut dolor blandit congue turpis eu accum
         san purus. Morbi viverra bibendum dui a dapibus sem aliquam vitae. Aliquam 
         sit amet consectetur sem sed cursus lorem. Aliquam et nisi vulputate placer
         at lorem in euismod lectus. Proin tempor tincidunt gravida. Maecenas a feug
         iat nulla. Donec tristique dolor et tellus sollicitudin ut malesuada odio r
         utrum. Mauris rutrum dolor libero id ullamcorper nibh vulputate vel. Nam im
         perdiet augue ut dui interdum sed sollicitudin felis lacinia. Fusce digniss
         im risus enim ac maximus leo tincidunt a. Quisque mattis tellus sed hendrer
         it molestie. Ut varius erat nunc non maximus sem malesuada sit amet. Vestib
         ulum id massa eu eros mollis consectetur at in ex. Vestibulum ultricies id 
         sem a molestie. Morbi ornare magna a sagittis pulvinar. Donec nec quam at o
         rci dictum dapibus tincidunt vulputate ligula. Etiam diam nibh facilisis eg
         et placerat et pellentesque sit amet nisi. Integer in nunc elementum pellen
         tesque justo eu pretium elit. Aliquam suscipit dapibus ornare. Praesent nec
          sem mi. Nunc cursus justo nec dignissim volutpat dui mauris cursus magna s
         ed maximus nisi ligula id nunc. Ut non neque sed nisi placerat fringilla. P
         hasellus commodo finibus semper. In hac habitasse platea dictumst. Duis non
          diam euismod sodales sem congue porttitor nunc. Ut at dapibus orci. Curabi
         tur euismod tincidunt eleifend. Nullam diam nibh rutrum id dolor non cursus
          dapibus leo. Aenean sed purus in elit eleifend finibus. Mauris luctus frin
         gilla eleifend. Duis et dui orci. In interdum interdum enim eget condimentu
         m odio porta aliquet. Maecenas non nisl mauris. Sed neque libero venenatis 
         id dolor nec rhoncus egestas ligula. In non felis suscipit cursus risus nec
          volutpat enim. Sed iaculis feugiat risus ac tempor. Quisque nec orci elit.
          Nullam hendrerit tellus velit et semper leo bibendum ac. Cras sagittis met
         us in vehicula fringilla sem mauris sodales magna vel sagittis nisi magna q
         uis odio. Donec arcu leo fringilla quis erat a rutrum eleifend diam. Ut lig
         ula mi sagittis ut porttitor non pulvinar a ipsum. Maecenas non lectus vel 
         nisl convallis euismod. Vestibulum egestas tincidunt convallis. Mauris nec 
         leo in felis sagittis dapibus. Fusce scelerisque pretium blandit. Sed preti
         um varius sapien quis vulputate. Vestibulum aliquet ultrices euismod. Duis 
         neque eros ornare a risus nec luctus mattis dolor. Proin eget consectetur n
         isl. Donec a quam sit amet dui gravida lobortis. Aenean quis tellus iaculis
          lacinia lacus nec eleifend ligula. Curabitur malesuada finibus enim id ege
         stas. Nullam leo magna blandit non tempor in laoreet ut mauris. Sed rutrum 
         pellentesque justo. Integer varius malesuada urna a facilisis. Donec bibend
         um maximus sapien. Morbi rhoncus rutrum massa id posuere. Duis maximus eu e
         x in cursus. Mauris dictum porttitor augue. Nam sed tortor lorem. Cras conv
         allis odio odio dignissim viverra sapien ultrices ut. Nunc pellentesque lac
         inia eros id lobortis nulla dapibus quis. Phasellus ac eleifend ante. Nunc 
         massa justo viverra non commodo at finibus eget augue. Pellentesque tincidu
         nt nisi eget molestie viverra odio orci semper ligula ornare pellentesque s
         em sem vulputate libero. Quisque tincidunt sapien quis enim tincidunt portt
         itor. Maecenas pulvinar eros.
        LAST-MODIFIED:20260720T233341Z
        SEQUENCE:0
        STATUS:CONFIRMED
        SUMMARY:Event with long description
        TRANSP:TRANSPARENT
        END:VEVENT
        END:VCALENDAR
        """;

    private static readonly StringSerializer serializer = new();

    private static readonly string ShortText = (string) serializer.Deserialize(ShortEscape)!;
    private static readonly string MediumText = (string) serializer.Deserialize(MediumEscape)!;
    private static readonly string LongText = (string) serializer.Deserialize(LongEscape)!;

    public static readonly string LongTextNoSpecial = Calendar.Load(EventWithLongDescription)!.Events[0]!.Description!;

    private static readonly List<string> _largestCalendars = GetAllCalendars();

    private static List<string> GetAllCalendars()
    {
        var testProjectDirectory = Runner.FindParentFolder("Ical.Net.Tests", Directory.GetCurrentDirectory());
        var topLevelIcsPath = Path.GetFullPath(Path.Combine(testProjectDirectory, "Calendars"));
        return Directory.EnumerateFiles(topLevelIcsPath, "*.ics", SearchOption.AllDirectories)
            .Where(p => !p.EndsWith("DateTime1.ics")) // contains a deliberate error
            .Select(File.ReadAllText)
            .OrderByDescending(s => s.Length)
            .Take(10)
            .ToList();
    }

    [Benchmark]
    public void LoadAndSerializeCalendars()
    {
        var s = new CalendarSerializer();

        foreach (var ics in _largestCalendars)
        {
            var items = Calendar.Load<Calendar>(ics);

            foreach (var cal in items)
            {
                s.SerializeToString(cal);
            }
        }
    }

    [Benchmark]
    public object? ShortUnescape() => serializer.Deserialize(ShortEscape);

    [Benchmark]
    public object? MediumUnescape() => serializer.Deserialize(MediumEscape);

    [Benchmark]
    public object? LongUnescape() => serializer.Deserialize(LongEscape);

    [Benchmark]
    public object? ShortNoEscapedChars() => serializer.Deserialize(ShortNoEscape);

    [Benchmark]
    public object? LongNoEscapedChars() => serializer.Deserialize(LongNoEscape);


    [Benchmark]
    public string? ShortSerializeEscape() => serializer.SerializeToString(ShortText);

    [Benchmark]
    public string? ShortSerializeNoSpecial() => serializer.SerializeToString(ShortNoEscape);

    [Benchmark]
    public string? MediumSerializeEscape() => serializer.SerializeToString(MediumText);

    [Benchmark]
    public string? LongSerializeEscape() => serializer.SerializeToString(LongText);

    [Benchmark]
    public string? LongSerializeNoSpecial() => serializer.SerializeToString(LongTextNoSpecial);

}
