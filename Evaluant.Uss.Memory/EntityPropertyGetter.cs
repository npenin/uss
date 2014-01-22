using System;
using System.Collections.Generic;
using System.Text;
using Evaluant.NLinq.Memory;
using Evaluant.Uss.Domain;
using System.Collections;
using Evaluant.Uss.PersistenceEngine.Contracts;

namespace Evaluant.Uss.Memory
{
    public class EntityPropertyGetter : IPropertyGetter
    {
        public EntityPropertyGetter(IPersistenceEngine engine)
        {
            model = engine.Factory.Model;
        }

        Model.Model model;

        CachedReflectionPropertyGetter innerGetter = new CachedReflectionPropertyGetter();
        #region IPropertyGetter Members

        public object GetValue(object obj, string propertyName)
        {
            if (obj is DictionaryEntry)
                return GetValue(((DictionaryEntry)obj).Value, propertyName);
            if (obj is Entry<Entity>)
            {
                return GetValue(((Entry<Entity>)obj).TypedValue, propertyName);
            }
            if (obj is Entity)
            {
                Entity e = ((Entity)obj);
                Entry ee = e[propertyName];
                if (ee == null)
                {
                    Model.Reference reference = model.GetReference(e.Type, propertyName);
                    if (reference != null && reference.ToMany)
                        return new List<object>();
                    return null;
                }
                return ee.IsMultiple ? ee : ee.Value;
            }
            return innerGetter.GetValue(obj, propertyName);
        }

        #endregion
    }
}
