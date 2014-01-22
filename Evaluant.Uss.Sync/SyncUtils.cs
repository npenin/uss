using System;
using System.Text;
using Evaluant.Uss.Commands;
using Evaluant.Uss.Domain;
//using Evaluant.Uss.Common;

namespace Evaluant.Uss.Sync
{
    public class SyncUtils
    {

        public const string COMMAND = "Evaluant:Uss:Sync:Command";
        public const string CONNECTION = "Evaluant:Uss:Sync:Connection";
        public const string INFO = "Evaluant:Uss:Sync:Info";

        public const string COMPOUND_CREATE = "Evaluant:Uss:Sync:CompoundCreateCommand";
        public const string COMPOUND_UPDATE = "Evaluant:Uss:Sync:CompoundUpdateCommand";
        public const string CREATE_ENTITY = "Evaluant:Uss:Sync:CreateEntityCommand";
        public const string DELETE_ENTITY = "Evaluant:Uss:Sync:DeleteEntityCommand";
        public const string CREATE_ATTRIBUTE = "Evaluant:Uss:Sync:CreateAttributeCommand";
        public const string UPDATE_ATTRIBUTE = "Evaluant:Uss:Sync:UpdateAttributeCommand";
        public const string DELETE_ATTRIBUTE = "Evaluant:Uss:Sync:DeleteAttributeCommand";
        public const string CREATE_REFERENCE = "Evaluant:Uss:Sync:CreateReferenceCommand";
        public const string DELETE_REFERENCE = "Evaluant:Uss:Sync:DeleteReferenceCommand";

        public const string CLIENTID = "ClientId";
        public const string PROCESSED = "Processed";
        public const string TRANSACTION = "Transaction";
        public const string NUMBER = "Number";
        public const string PARENTID = "ParentId";
        public const string TYPE = "Type";
        public const string PARENTTYPE = "ParentType";
        public const string NAME = "Name";
        public const string VALUE = "Value";
        public const string CHILDID = "ChildId";
        public const string CHILDTYPE = "ChildType";
        public const string ROLE = "Role";

        //public static Command CreateCommand(Entity e)
        //{
        //    switch (e.Type)
        //    {
        //        case SyncUtils.CREATE_ENTITY:
        //            CreateEntityCommand ce = new CreateEntityCommand(
        //                e.GetString(SyncUtils.PARENTID),
        //                e.GetString(SyncUtils.TYPE)
        //                );
        //            return ce;

        //        case SyncUtils.DELETE_ENTITY:
        //            DeleteEntityCommand de = new DeleteEntityCommand(
        //                e.GetString(SyncUtils.PARENTID),
        //                e.GetString(SyncUtils.TYPE)
        //                );
        //            return de;

        //        case SyncUtils.CREATE_ATTRIBUTE:
        //            CreateAttributeCommand ca = new CreateAttributeCommand(
        //                e.GetString(SyncUtils.PARENTID),
        //                e.GetString(SyncUtils.PARENTTYPE),
        //                e.GetString(SyncUtils.NAME),
        //                Factory.TypeResolver.GetType(e.GetString(SyncUtils.TYPE)),
        //                Utils.UnSerialize(e.GetString(SyncUtils.VALUE))
        //                );
        //            return ca;

        //        case SyncUtils.DELETE_ATTRIBUTE:
        //            DeleteAttributeCommand da = new DeleteAttributeCommand(
        //                e.GetString(SyncUtils.PARENTID),
        //                e.GetString(SyncUtils.PARENTTYPE),
        //                e.GetString(SyncUtils.NAME),
        //                Factory.TypeResolver.GetType(e.GetString(SyncUtils.TYPE)),
        //                null
        //                );
        //            return da;

        //        case SyncUtils.UPDATE_ATTRIBUTE:
        //            UpdateAttributeCommand ua = new UpdateAttributeCommand(
        //                e.GetString(SyncUtils.PARENTID),
        //                e.GetString(SyncUtils.PARENTTYPE),
        //                e.GetString(SyncUtils.NAME),
        //                Factory.TypeResolver.GetType(e.GetString(SyncUtils.TYPE)),
        //                Utils.UnSerialize(e.GetString(SyncUtils.VALUE))
        //                );
        //            return ua;

        //        case SyncUtils.CREATE_REFERENCE:
        //            CreateReferenceCommand cr = new CreateReferenceCommand(
        //                e.GetString(SyncUtils.ROLE),
        //                e.GetString(SyncUtils.PARENTID),
        //                e.GetString(SyncUtils.PARENTTYPE),
        //                e.GetString(SyncUtils.CHILDID),
        //                e.GetString(SyncUtils.CHILDTYPE)
        //                );
        //            return cr;

        //        case SyncUtils.DELETE_REFERENCE:
        //            DeleteReferenceCommand dr = new DeleteReferenceCommand(
        //                e.GetString(SyncUtils.ROLE),
        //                e.GetString(SyncUtils.PARENTID),
        //                e.GetString(SyncUtils.PARENTTYPE),
        //                e.GetString(SyncUtils.CHILDID),
        //                e.GetString(SyncUtils.CHILDTYPE)
        //                );
        //            return dr;

        //        default:
        //            throw new UniversalStorageException("Unexpected command type");
        //    }
        //}
    }
}
