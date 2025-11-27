using System.Data;
using System.Linq.Expressions;
using EM.Domain.Interface;
using EM.Repository.Interfaces;
using EM.Repository.Banco;

namespace EM.Repository
{
    public abstract class RepositorioAbstrato<T> : IRepositorioBase<T> where T : IEntidade
    {
        protected abstract string TableName { get; }
        protected abstract string PrimaryKeyColumn { get; }
        protected abstract T MapFromReader(IDataReader dataReader);
        protected abstract void AddInsertParameters(IDbCommand cmd, T objeto);
        protected abstract void AddUpdateParameters(IDbCommand cmd, T objeto);

        public virtual void Add(T entidade)
        {
            using IDbConnection conexao = DBHelper.Instancia.CrieConexao();
            conexao.Open();
            using IDbCommand comando = DBHelper.Instancia.CreateCommand(conexao);

            comando.CommandText = GetInsertCommand();
            AddInsertParameters(comando, entidade);
            comando.ExecuteNonQuery();
        }

        public virtual void Remove(T entidade)
        {
            using IDbConnection conexao = DBHelper.Instancia.CrieConexao();
            conexao.Open();
            using IDbCommand comando = DBHelper.Instancia.CreateCommand(conexao);

            comando.CommandText = $"DELETE FROM {TableName} WHERE {PrimaryKeyColumn} = @Id";
            AddDeleteParameters(comando, entidade);
            comando.ExecuteNonQuery();
        }

        public virtual void Update(T entidade)
        {
            using IDbConnection conexao = DBHelper.Instancia.CrieConexao();
            conexao.Open();
            using IDbCommand comando = DBHelper.Instancia.CreateCommand(conexao);

            comando.CommandText = GetUpdateCommand();
            AddUpdateParameters(comando, entidade);
            comando.ExecuteNonQuery();
        }

        public virtual IEnumerable<T> GetAll()
        {
            List<T> listaEntidades = new List<T>();

            using IDbConnection conexao = DBHelper.Instancia.CrieConexao();
            conexao.Open();
            using IDbCommand comando = DBHelper.Instancia.CreateCommand(conexao);

            comando.CommandText = $"SELECT * FROM {TableName}";
            using IDataReader dataReader = comando.ExecuteReader();

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

        public virtual T? GetById(int id)
        {
            using IDbConnection conexao = DBHelper.Instancia.CrieConexao();
            conexao.Open();
            using IDbCommand comando = DBHelper.Instancia.CreateCommand(conexao);

            comando.CommandText = $"SELECT * FROM {TableName} WHERE {PrimaryKeyColumn} = @Id";
            comando.CreateParameter("@Id", id);

            using IDataReader dataReader = comando.ExecuteReader();
            if (dataReader.Read())
            {
                return MapFromReader(dataReader);
            }

            return default;
        }

        public virtual bool Exists(int id)
        {
            using IDbConnection cn = DBHelper.Instancia.CrieConexao();
            cn.Open();
            using IDbCommand cmd = DBHelper.Instancia.CreateCommand(cn);

            cmd.CommandText = $"SELECT 1 FROM {TableName} WHERE {PrimaryKeyColumn} = @Id";
            cmd.CreateParameter("@Id", id);

            using IDataReader reader = cmd.ExecuteReader();
            return reader.Read();
        }

        public virtual int Count()
        {
            using IDbConnection cn = DBHelper.Instancia.CrieConexao();
            cn.Open();
            using IDbCommand cmd = DBHelper.Instancia.CreateCommand(cn);

            cmd.CommandText = $"SELECT COUNT(*) FROM {TableName}";
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        protected abstract string GetInsertCommand();
        protected abstract string GetUpdateCommand();
        protected abstract void AddDeleteParameters(IDbCommand cmd, T objeto);
    }
}
