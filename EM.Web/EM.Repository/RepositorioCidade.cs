using System.Data;
using EM.Domain;
using EM.Domain.Utilitarios;
using EM.Repository.Banco;

namespace EM.Repository
{
    public class RepositorioCidade : RepositorioAbstrato<Cidade>
    {
        protected override string TableName => "TBCIDADE";
        protected override string PrimaryKeyColumn => "CIDCODIGO";

        protected override string GetInsertCommand()
        {
            return @"INSERT INTO TBCIDADE (CIDNOME, CIDUF) 
                    VALUES (@Nome, @UF)";
        }

        protected override string GetUpdateCommand()
        {
            return @"UPDATE TBCIDADE SET 
                    CIDNOME = @Nome, 
                    CIDUF = @UF 
                    WHERE CIDCODIGO = @Codigo";
        }

        protected override void AddInsertParameters(IDbCommand cmd, Cidade cidade)
        {
            cmd.CreateParameter("@Nome", cidade.Nome);
            cmd.CreateParameter("@UF", cidade.UF?.ToUpper());
        }

        protected override void AddUpdateParameters(IDbCommand cmd, Cidade cidade)
        {
            AddInsertParameters(cmd, cidade);
            cmd.CreateParameter("@Codigo", cidade.Codigo);
        }

        protected override void AddDeleteParameters(IDbCommand cmd, Cidade cidade)
        {
            cmd.CreateParameter("@Id", cidade.Codigo);
        }

        protected override Cidade MapFromReader(IDataReader dataReader)
        {
            return new Cidade
            {
                Codigo = dataReader["CIDCODIGO"].ToObject<int>(),
                Nome = dataReader["CIDNOME"].ToObject<string>(),
                UF = dataReader["CIDUF"].ToObject<string>()
            };
        }

        public Cidade GetByCodigo(int codigo)
        {
            return GetById(codigo);
        }

        public IEnumerable<Cidade> GetByUF(string uf)
        {
            return Get(c => c.UF == uf.ToUpper());
        }

        public IEnumerable<Cidade> GetByNome(string nome)
        {
            var listaCidades = new List<Cidade>();

            using var conexao = DBHelper.Instancia.CrieConexao();
            conexao.Open();
            using var comando = DBHelper.Instancia.CreateCommand(conexao);

            comando.CommandText = "SELECT * FROM TBCIDADE WHERE UPPER(CIDNOME) CONTAINING UPPER(@Nome) ORDER BY CIDNOME";
            comando.CreateParameter("@Nome", nome);

            using var dataReader = comando.ExecuteReader();
            while (dataReader.Read())
            {
                listaCidades.Add(MapFromReader(dataReader));
            }

            return listaCidades;
        }

        public IEnumerable<string> GetUFs()
        {
            var listaUF = new List<string>();

            using var conexao = DBHelper.Instancia.CrieConexao();
            conexao.Open();
            using var comando = DBHelper.Instancia.CreateCommand(conexao);

            comando.CommandText = "SELECT DISTINCT CIDUF FROM TBCIDADE ORDER BY CIDUF";

            using var dataReader = comando.ExecuteReader();
            while (dataReader.Read())
            {
                listaUF.Add(dataReader["CIDUF"].ToObject<string>());
            }

            return listaUF;
        }

        public bool CidadeTemAlunos(int codigoCidade)
        {
            using var conexao = DBHelper.Instancia.CrieConexao();
            conexao.Open();
            using var comando = DBHelper.Instancia.CreateCommand(conexao);

            comando.CommandText = "SELECT 1 FROM TBALUNO WHERE ALUCODCIDADE = @CodigoCidade";
            comando.CreateParameter("@CodigoCidade", codigoCidade);

            using var dataReader = comando.ExecuteReader();
            return dataReader.Read();
        }
    }
}