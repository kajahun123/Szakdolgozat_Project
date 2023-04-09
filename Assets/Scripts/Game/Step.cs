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
            Movement
        }
        public UnitScript changingUnit;
        public UnitScript attackerUnit;
        public Type type;
        public int score;

        public Step(Type type, UnitScript changingUnit,  int score, UnitScript attackerUnit = null )
        {
            this.changingUnit = changingUnit;
            this.attackerUnit = attackerUnit;
            this.type = type;
            this.score = score;
        }
    }
}
