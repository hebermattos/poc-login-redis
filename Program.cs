using System;
using System.Linq;
using System.Threading.Tasks;
using StackExchange.Redis;

namespace redis_set
{
    class Program
    {
       

        static async Task Main(string[] args)
        {
            ConnectionMultiplexer redis = ConnectionMultiplexer.Connect("localhost");
            IDatabase db = redis.GetDatabase();

            db.KeyDelete("edsonlp");

            var g1 = "0572af40-ebc3-43df-ae3d-51883a7ea660";
            var g2 = "7c595a60-e198-477c-a0d8-2d59945c6724";
            var g3 = "35cb6b9b-523e-4d2c-a6ec-b963de7d93be";
            var g4 = "2fe03cd0-1f6a-4bb2-a8b1-b6666957edda";
            var g5 = "660d2bab-5948-491a-9fe9-9db047580a5a";
            var g6 = "18017b25-1cf1-4ea3-aafc-49e86d210abe";

            //primeiros 5 logins

            db.HashSet("edsonlp", g1, DateTime.Now.ToString());
            db.HashSet("edsonlp", g2, DateTime.Now.ToString());
            db.HashSet("edsonlp", g3, DateTime.Now.ToString());
            db.HashSet("edsonlp", g4, DateTime.Now.ToString());
            db.HashSet("edsonlp", g5, DateTime.Now.AddDays(-10).ToString());

            // -------------------- auth
            await PodeLogar(db, g6);
            //se veio true, pode gerar token

            // -------------------- servicos

            //nao vai atualizar pq chave nao existe. 
            var tran3 = db.CreateTransaction();
            tran3.AddCondition(Condition.HashExists("edsonlp", g5));
            tran3.HashSetAsync("edsonlp", g5, DateTime.Now.AddDays(10).ToString());
            bool committed3 = tran3.Execute();

            //vai atualizar a chave criada dentro do PodeLogar
            var tran2 = db.CreateTransaction();
            tran2.AddCondition(Condition.HashExists("edsonlp", g6));
            tran2.HashSetAsync("edsonlp", g6, DateTime.Now.AddYears(1).ToString());
            bool committed2 = tran2.Execute();

            Listar(db);
        }

        private static async Task<bool> PodeLogar(IDatabase db, string g6)
        {     

            var podeLogar = false;

            var tran4 = db.CreateTransaction();
            tran4.AddCondition(Condition.HashLengthLessThan("edsonlp", 5));
            tran4.HashSetAsync("edsonlp", g6, DateTime.Now.AddYears(1).ToString());
            bool committed2 = tran4.Execute();

            podeLogar = committed2;

            if (podeLogar == false)
            {    
                var loginExpirado = (await db.HashGetAllAsync("edsonlp")).Select(x => new Login
                {
                    JwtID = x.Name,
                    UltimoAcesso = Convert.ToDateTime(x.Value)
                }).FirstOrDefault(x => x.UltimoAcesso.AddMinutes(5).CompareTo(DateTime.Now) < 0);

                if (loginExpirado != null)
                {                    
                    var tran = db.CreateTransaction();
                    tran.AddCondition(Condition.HashExists("edsonlp", loginExpirado.JwtID));
                    tran.HashDeleteAsync("edsonlp", loginExpirado.JwtID);
                    tran.HashSetAsync("edsonlp", g6, DateTime.Now.AddYears(1).ToString());
                    bool committed = tran.Execute();

                    podeLogar = committed;
                }
            }

            return podeLogar;
        }

        private static void Listar(IDatabase db)
        {
            var itens = db.HashGetAll("edsonlp").Select(x => x.Name + " - " + x.Value);

            foreach (var item in itens)
            {
                Console.WriteLine(item);
            }

             Console.WriteLine();
        }
    }
}

