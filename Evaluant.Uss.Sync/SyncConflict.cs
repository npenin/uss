using System;
using System.Text;

namespace Evaluant.Uss.Sync
{
    /// If an enity is deleted while it has been modified on the other side then the priority goes to
    /// the deletion
   
    public enum SyncConflict
    {
        ClientUpdateServerUpdate // The client and the server updated the same attribute
    }
}
