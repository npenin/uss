using System;

namespace Evaluant.Uss.Utility
{
    public delegate void PropertyChangingEventHandler(object sender, PropertyChangingEventArgs e);

    public interface INotifyPropertyChanging
    {
        event PropertyChangingEventHandler PropertyChanging;
    }
}
