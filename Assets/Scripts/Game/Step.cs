using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Scripts.Game
{
    public class Step
    {
        public enum Type
        {
            Attack,
            Move
        }
        public UnitScript unit;
        public Type type;

        public Step(UnitScript unit, Type type)
        {
            this.unit = unit;
            this.type = type;
        }
    }
}
