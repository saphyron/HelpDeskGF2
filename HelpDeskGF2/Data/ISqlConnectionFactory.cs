using System.Data;

namespace HelpDesk.Data;

    public interface ISqlConnectionFactory
    {
        IDbConnection Create();
    }
