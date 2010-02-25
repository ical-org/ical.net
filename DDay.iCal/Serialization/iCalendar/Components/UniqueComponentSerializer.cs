using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using DDay.iCal;
using DDay.iCal;

namespace DDay.iCal.Serialization
{
    public class UniqueComponentSerializer : ComponentSerializer
    {
        #region Protected Properties

        protected UniqueComponent UniqueComponent
        {
            get { return Component as UniqueComponent; }
        }

        #endregion

        #region Constructors

        public UniqueComponentSerializer(UniqueComponent uc) : base(uc) {}
        
        #endregion

        #region Overrides

        public override void Serialize(Stream stream, Encoding encoding)
        {
            if (UniqueComponent != null)
            {
                if (UniqueComponent.Categories != null)
                {
                    foreach (TextCollection tc in UniqueComponent.Categories)
                        tc.Name = "CATEGORIES";
                }
                if (UniqueComponent.Attach != null)
                {
                    foreach (Binary b in UniqueComponent.Attach)
                        b.Name = "ATTACH";
                }
                if (UniqueComponent.Comment != null)
                {
                    foreach (Text t in UniqueComponent.Comment)
                        t.Name = "COMMENT";
                }
                if (UniqueComponent.Contact != null)
                {
                    foreach (Text t in UniqueComponent.Contact)
                        t.Name = "CONTACT";
                }
                if (UniqueComponent.Related_To != null)
                {
                    foreach (Text t in UniqueComponent.Related_To)
                        t.Name = "RELATED-TO";
                }
                if (UniqueComponent.Request_Status != null)
                {
                    foreach (RequestStatus rs in UniqueComponent.Request_Status)
                        rs.Name = "REQUEST-STATUS";
                }
            }

            base.Serialize(stream, encoding);
        }

        #endregion
    }
}
