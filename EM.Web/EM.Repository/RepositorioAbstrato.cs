using System.Data;
using System.Linq.Expressions;
using EM.Domain.Interface;
using EM.Repository.Banco;

namespace EM.Repository
{
    public abstract class RepositorioAbstrato<T> where T : IEntidade
    {
        protected abstract string TableName { get; }
        protected abstract string PrimaryKeyColumn { get; }
        protected abstract T MapFromReader(IDataReader dataReader);
        protected abstract void AddInsertParameters(IDbCommand cmd, T objeto);
        protected abstract void AddUpdateParameters(IDbCommand cmd, T objeto);

        public virtual void Add(T entidade)
        {
            using var conexao = DBHelper.Instancia.CrieConexao();
            conexao.Open();
            using var comando = DBHelper.Instancia.CreateCommand(conexao);

            comando.CommandText = GetInsertCommand();
            AddInsertParameters(comando, entidade);
            comando.ExecuteNonQuery();
        }

        public virtual void Remove(T entidade)
        {
            using var conexao = DBHelper.Instancia.CrieConexao();
            conexao.Open();
            using var comando = DBHelper.Instancia.CreateCommand(conexao);

            comando.CommandText = $"DELETE FROM {TableName} WHERE {PrimaryKeyColumn} = @Id";
            AddDeleteParameters(comando, entidade);
            comando.ExecuteNonQuery();
        }

        public virtual void Update(T entidade)
        {
            using var conexao = DBHelper.Instancia.CrieConexao();
            conexao.Open();
            using var comando = DBHelper.Instancia.CreateCommand(conexao);

            comando.CommandText = GetUpdateCommand();
            AddUpdateParameters(comando, entidade);
            comando.ExecuteNonQuery();
        }

        public virtual IEnumerable<T> GetAll()
        {
            var listaEntidades = new List<T>();

            using var conexao = DBHelper.Instancia.CrieConexao();
            conexao.Open();
            using var comando = DBHelper.Instancia.CreateCommand(conexao);

            comando.CommandText = $"SELECT * FROM {TableName}";
            using var dataReader = comando.ExecuteReader();

            while (dataReader.Read())
            {
                listaEntidades.Add(MapFromReader(dataReader));
            }

            return listaEntidades;
        }

        public virtual IEnumerable<T> Get(Expression<Func<T, bool>> predicate)
        {
            return GetAll().AsQueryable().Where(predicate);
        }

        public virtual T GetById(int id)
        {
            using var conexao = DBHelper.Instancia.CrieConexao();
            conexao.Open();
            using var comando = DBHelper.Instancia.CreateCommand(conexao);

            comando.CommandText = $"SELECT * FROM {TableName} WHERE {PrimaryKeyColumn} = @Id";
            comando.CreateParameter("@Id", id);

            using var dataReader = comando.ExecuteReader();
            if (dataReader.Read())
            {
                return MapFromReader(dataReader);
            }

            return default(T);
        }

        public virtual bool Exists(int id)
        {
            using var cn = DBHelper.Instancia.CrieConexao();
            cn.Open();
            using var cmd = DBHelper.Instancia.CreateCommand(cn); // CORRIGIDO

            cmd.CommandText = $"SELECT 1 FROM {TableName} WHERE {PrimaryKeyColumn} = @Id";
            cmd.CreateParameter("@Id", id);

            using var reader = cmd.ExecuteReader();
            return reader.Read();
        }

        public virtual int Count()
        {
            using var cn = DBHelper.Instancia.CrieConexao();
            cn.Open();
            using var cmd = DBHelper.Instancia.CreateCommand(cn); // CORRIGIDO

            cmd.CommandText = $"SELECT COUNT(*) FROM {TableName}";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        protected abstract string GetInsertCommand();
        protected abstract string GetUpdateCommand();
        protected abstract void AddDeleteParameters(IDbCommand cmd, T objeto);
    }
}