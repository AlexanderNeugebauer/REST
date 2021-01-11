using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
   class SpellCard : Card
   {
      public SpellCard(object[] data) : base(data)
      {
      }

      override public double CalcDamage(Card opponent)
      {
         double ret = Damage;
         switch (CardElement)
         {
            case Element.water:
               switch (opponent.CardElement)
               {
                  case Element.fire:
                     ret *= 2;
                     break;
                  case Element.normal:
                     ret /= 2;
                     break;
               }
               break;
            case Element.fire:
               switch (opponent.CardElement)
               {
                  case Element.water:
                     ret /= 2;
                     break;
                  case Element.normal:
                     ret *= 2;
                     break;
               }
               break;
            case Element.normal:
               switch (opponent.CardElement)
               {
                  case Element.water:
                     ret *= 2;
                     break;
                  case Element.fire:
                     ret /= 2;
                     break;
               }
               break;
         }
         return ret;
      }
   }
}