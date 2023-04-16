using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Scripts.Game
{
    public class UnitState
    {
        public int healthPoint;
        public int x;
        public int y;

        public UnitState(UnitState other)
        {
            this.healthPoint = other.healthPoint;
            this.x = other.x;
            this.y = other.y;
        }

        public UnitState(UnitScript unit)
        {
            this.healthPoint = unit.currentHealthPoints;
            this.x = unit.x;
            this.y = unit.y;
        }
    }
}
