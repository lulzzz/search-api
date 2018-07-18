using System;
using System.Collections.Generic;
using System.Text;

namespace SearchV2.RDKit
{
    public interface IPostgresConnectionInfo
    {
        string PostgresHost { get; }
        string PostgresDbName { get; }
        string PostgresPassword { get; }
    }
}
