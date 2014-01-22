using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Evaluant.Uss.SqlMapper.Mapping
{
    public class CompositeReference : Reference
    {
        public override bool IsComposite
        {
            get
            {
                return true;
            }
        }

        public override void Initialize(bool needsModel)
        {
            if (Target == null)
            {
                Target = Mapping[ChildType];
                Target.Mapping = Mapping;
            }
            Target.Initialize(needsModel);

            if (Rules.Count == 0)
                Rules.Add(new Rule()
                {
                    ParentTable = Parent.Table,
                    ParentFieldNames = string.Join(",", Parent.Table.Ids),
                    ChildTable = Parent.Table,
                    ChildFieldNames = string.Join(",", Parent.Table.Ids)
                });
            if (string.IsNullOrEmpty(Rules[0].ParentTableName))
                Rules[0].ParentTableName = Parent.TableName;
            foreach (Rule rule in Rules)
            {
                rule.Mapping = Mapping;
                rule.Initialize();
            }

        }
    }
}
