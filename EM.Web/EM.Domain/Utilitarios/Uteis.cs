
using System.Diagnostics.CodeAnalysis;

namespace EM.Domain.Utilitarios
{
    public static class Uteis
    {
        public static object ToBD(this object valor)
        {
            if (valor == null ||
                (valor is string texto && string.IsNullOrWhiteSpace(texto)) ||
                (valor is int inteiro && inteiro == 0) ||
                (valor is DateTime data && data == DateTime.MinValue))
            {
                return DBNull.Value;
            }
            return valor;
        }

        [return: MaybeNull]
        public static T ToObject<T>(this object valor)
        {
            if (valor == null || valor == DBNull.Value)
                return default!;

            var tipoDestino = typeof(T);
            var tipoSubjacenteNullable = Nullable.GetUnderlyingType(tipoDestino);
            if (tipoSubjacenteNullable != null)
                return (T)Convert.ChangeType(valor, tipoSubjacenteNullable);

            return (T)Convert.ChangeType(valor, tipoDestino);
        }

        public static DateTime? ToDateTimeNullable(this object valor)
        {
            if (valor == null || valor == DBNull.Value)
                return null;

            return Convert.ToDateTime(valor);
        }
    }
}