using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG
{
   class Card
   {
      public string ID { get; }
      public string Name { get; }
      public double Damage { get; }
      public enum Element
      {
         water,
         fire,
         normal
      }
      public Element CardElement { get; }
      public Card(Object[] data)
      {
         ID = data[0].ToString();
         Name = data[1].ToString();
         
         Damage = Convert.ToDouble(data[2].ToString());
      }

      public string ToJson()
      {
         return $"{{'Id': '{ID}', 'Name': '{Name}', 'Damage': {Damage.ToString()}}}";
      }

      public static string CollectionToJson(List<Card> collection)
      {
         string ret = "[";

         foreach (Card card in collection)
         {
            ret += card.ToJson() + ", ";
         }
         ret = ret.Substring(0, ret.Length - 2);
         ret += "]";

         return ret;
      }

      virtual public double CalcDamage(Card opponent)
      {
         return Damage;
      }
   }
}
