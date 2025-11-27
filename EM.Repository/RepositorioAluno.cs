using System.Data;
using EM.Domain;
using EM.Domain.Utilitarios;
using EM.Repository.Banco;

using EM.Repository.Interfaces;

namespace EM.Repository
{
    public class RepositorioAluno : RepositorioAbstrato<Aluno>, IRepositorioAluno
    {
        protected override string TableName => "TBALUNO";
        protected override string PrimaryKeyColumn => "ALUMATRICULA";

        protected override string GetInsertCommand()
        {
            return @"INSERT INTO TBALUNO 
                    (ALUNOME, ALUCPF, ALUNASCIMENTO, ALUSEXO, ALUCODCIDADE) 
                    VALUES (@Nome, @CPF, @Nascimento, @Sexo, @CidadeCodigo)";
        }

        protected override string GetUpdateCommand()
        {
            return @"UPDATE TBALUNO SET 
                    ALUNOME = @Nome, 
                    ALUCPF = @CPF, 
                    ALUNASCIMENTO = @Nascimento, 
                    ALUSEXO = @Sexo, 
                    ALUCODCIDADE = @CidadeCodigo 
                    WHERE ALUMATRICULA = @Matricula";
        }

        protected override void AddInsertParameters(IDbCommand cmd, Aluno aluno)
        {
            cmd.CreateParameter("@Nome", aluno.Nome);
            cmd.CreateParameter("@CPF", aluno.CPF);
            cmd.CreateParameter("@Nascimento", aluno.Nascimento);
            cmd.CreateParameter("@Sexo", (int)aluno.Sexo);
            cmd.CreateParameter("@CidadeCodigo", aluno.CidadeCodigo);
        }

        protected override void AddUpdateParameters(IDbCommand cmd, Aluno aluno)
        {
            AddInsertParameters(cmd, aluno);
            cmd.CreateParameter("@Matricula", aluno.Matricula);
        }

        protected override void AddDeleteParameters(IDbCommand cmd, Aluno aluno)
        {
            cmd.CreateParameter("@Id", aluno.Matricula);
        }

        protected override Aluno MapFromReader(IDataReader dataReader)
        {
            return new Aluno
            {
                Matricula = dataReader["ALUMATRICULA"].ToObject<int>(),
                Nome = dataReader["ALUNOME"].ToObject<string>() ?? string.Empty,
                CPF = dataReader["ALUCPF"].ToObject<string>() ?? string.Empty,
                Nascimento = dataReader["ALUNASCIMENTO"].ToObject<DateTime>(),
                Sexo = (EnumeradorSexo)dataReader["ALUSEXO"].ToObject<int>(),
                CidadeCodigo = dataReader["ALUCODCIDADE"].ToObject<int?>()
            };
        }

        public Aluno? GetByMatricula(int matricula)
        {
            return GetById(matricula);
        }

        public IEnumerable<Aluno> GetByConteudoNoNome(string termoNome)
        {
            List<Aluno> listaAlunos = [];

            using IDbConnection conexao = DBHelper.Instancia.CrieConexao();
            conexao.Open();
            using IDbCommand comando = DBHelper.Instancia.CreateCommand(conexao);

            comando.CommandText = @"
                SELECT A.*, C.CIDNOME, C.CIDUF 
                FROM TBALUNO A
                LEFT JOIN TBCIDADE C ON A.ALUCODCIDADE = C.CIDCODIGO
                WHERE UPPER(A.ALUNOME) CONTAINING UPPER(@Nome)
                ORDER BY A.ALUNOME";

            comando.CreateParameter("@Nome", termoNome);

            using IDataReader dataReader = comando.ExecuteReader();
            while (dataReader.Read())
            {
                Aluno aluno = MapFromReader(dataReader);
                aluno.CidadeNome = dataReader["CIDNOME"]?.ToObject<string>() ?? string.Empty;
                aluno.UF = dataReader["CIDUF"]?.ToObject<string>() ?? string.Empty;
                listaAlunos.Add(aluno);
            }

            return listaAlunos;
        }

        public Aluno? GetByCPF(string cpf)
        {
            using IDbConnection conexao = DBHelper.Instancia.CrieConexao();
            conexao.Open();
            using IDbCommand comando = DBHelper.Instancia.CreateCommand(conexao);

            comando.CommandText = "SELECT * FROM TBALUNO WHERE ALUCPF = @CPF";
            comando.CreateParameter("@CPF", cpf);

            using IDataReader dataReader = comando.ExecuteReader();
            if (dataReader.Read())
            {
                return MapFromReader(dataReader);
            }

            return null;
        }

        public IEnumerable<Aluno> GetBySexo(EnumeradorSexo sexo)
        {
            return Get(a => a.Sexo == sexo);
        }

        public IEnumerable<Aluno> GetByCidade(int codigoCidade)
        {
            return Get(a => a.CidadeCodigo == codigoCidade);
        }

        public bool CPFExiste(string cpf, int? matriculaExcluir = null)
        {
            using IDbConnection conexao = DBHelper.Instancia.CrieConexao();
            conexao.Open();
            using IDbCommand comando = DBHelper.Instancia.CreateCommand(conexao);

            if (matriculaExcluir.HasValue)
            {
                comando.CommandText = "SELECT 1 FROM TBALUNO WHERE ALUCPF = @CPF AND ALUMATRICULA != @Matricula";
                comando.CreateParameter("@Matricula", matriculaExcluir.Value);
            }
            else
            {
                comando.CommandText = "SELECT 1 FROM TBALUNO WHERE ALUCPF = @CPF";
            }

            comando.CreateParameter("@CPF", cpf);

            using IDataReader dataReader = comando.ExecuteReader();
            return dataReader.Read();
        }
    }
}
