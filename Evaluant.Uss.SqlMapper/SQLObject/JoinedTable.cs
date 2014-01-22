using SQLObject.Renderer;
using Evaluant.Uss.SqlMapper;

namespace SQLObject
{
    /// <summary>
    /// Description résumée de JoinedTable.
    /// </summary>
    public class JoinedTable : Table
    {
        private Table _LeftTable;
        private Table _RigthTable;

        private TypeJoinedTable _Type;

        public JoinedTable(Table leftTable, TypeJoinedTable type, Table rigthTable)
            : base(leftTable.TagMapping, leftTable.TableAlias)
        {
            _LeftTable = leftTable;
            _Type = type;
            _RigthTable = rigthTable;
        }

        public Table LeftTable
        {
            get { return _LeftTable; }
            set { _LeftTable = value; }
        }

        public Table RigthTable
        {
            get { return _RigthTable; }
            set { _RigthTable = value; }
        }

        public TypeJoinedTable Type
        {
            get { return _Type; }
        }

        private LogicExpressionCollection _SearchConditions = new LogicExpressionCollection();
        public LogicExpressionCollection SearchConditions
        {
            get { return _SearchConditions; }
            set { _SearchConditions = value; }
        }

        public override void Accept(ISQLVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return LeftTable.ToString() + " " + Type + " JOIN " + RigthTable.ToString() + " ON " + SearchConditions.ToString();
        }
    }

    public enum TypeJoinedTable { Inner, LeftOuter, RightOuter }
}
