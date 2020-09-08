using System;
using System.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace QuickJS.Extra.Sqlite.Native
{
    /*
    ** CAPI3REF: Result Codes
    ** KEYWORDS: {result code definitions}
    **
    ** Many SQLite functions return an integer result code from the set shown
    ** here in order to indicate success or failure.
    **
    ** New error codes may be added in future versions of SQLite.
    **
    ** See also: [extended result code definitions]
    */
    public enum ResultCode : int
    {
        OK = 0,   /* Successful result */
        /* beginning-of-error-codes */
        ERROR = 1,   /* Generic error */
        INTERNAL = 2,   /* Internal logic error in SQLite */
        PERM = 3,   /* Access permission denied */
        ABORT = 4,   /* Callback routine requested an abort */
        BUSY = 5,   /* The database file is locked */
        LOCKED = 6,   /* A table in the database is locked */
        NOMEM = 7,   /* A malloc() failed */
        READONLY = 8,   /* Attempt to write a readonly database */
        INTERRUPT = 9,   /* Operation terminated by sqlite3_interrupt()*/
        IOERR = 10,   /* Some kind of disk I/O error occurred */
        CORRUPT = 11,   /* The database disk image is malformed */
        NOTFOUND = 12,   /* Unknown opcode in sqlite3_file_control() */
        FULL = 13,   /* Insertion failed because database is full */
        CANTOPEN = 14,   /* Unable to open the database file */
        PROTOCOL = 15,   /* Database lock protocol error */
        EMPTY = 16,   /* Internal use only */
        SCHEMA = 17,   /* The database schema changed */
        TOOBIG = 18,   /* String or BLOB exceeds size limit */
        CONSTRAINT = 19,   /* Abort due to constraint violation */
        MISMATCH = 20,   /* Data type mismatch */
        MISUSE = 21,   /* Library used incorrectly */
        NOLFS = 22,   /* Uses OS features not supported on host */
        AUTH = 23,   /* Authorization denied */
        FORMAT = 24,   /* Not used */
        RANGE = 25,   /* 2nd parameter to sqlite3_bind out of range */
        NOTADB = 26,   /* File opened that is not a database file */
        NOTICE = 27,   /* Notifications from sqlite3_log() */
        WARNING = 28,   /* Warnings from sqlite3_log() */
        ROW = 100,  /* sqlite3_step() has another row ready */
        DONE = 101,  /* sqlite3_step() has finished executing */
        /* end-of-error-codes */

        /*
        ** CAPI3REF: Extended Result Codes
        ** KEYWORDS: {extended result code definitions}
        **
        ** In its default configuration, SQLite API routines return one of 30 integer
        ** [result codes].  However, experience has shown that many of
        ** these result codes are too coarse-grained.  They do not provide as
        ** much information about problems as programmers might like.  In an effort to
        ** address this, newer versions of SQLite (version 3.3.8 [dateof:3.3.8]
        ** and later) include
        ** support for additional result codes that provide more detailed information
        ** about errors. These [extended result codes] are enabled or disabled
        ** on a per database connection basis using the
        ** [sqlite3_extended_result_codes()] API.  Or, the extended code for
        ** the most recent error can be obtained using
        ** [sqlite3_extended_errcode()].
        */
        ERROR_MISSING_COLLSEQ = (ERROR | (1 << 8)),
        ERROR_RETRY = (ERROR | (2 << 8)),
        ERROR_SNAPSHOT = (ERROR | (3 << 8)),
        IOERR_READ = (IOERR | (1 << 8)),
        IOERR_SHORT_READ = (IOERR | (2 << 8)),
        IOERR_WRITE = (IOERR | (3 << 8)),
        IOERR_FSYNC = (IOERR | (4 << 8)),
        IOERR_DIR_FSYNC = (IOERR | (5 << 8)),
        IOERR_TRUNCATE = (IOERR | (6 << 8)),
        IOERR_FSTAT = (IOERR | (7 << 8)),
        IOERR_UNLOCK = (IOERR | (8 << 8)),
        IOERR_RDLOCK = (IOERR | (9 << 8)),
        IOERR_DELETE = (IOERR | (10 << 8)),
        IOERR_BLOCKED = (IOERR | (11 << 8)),
        IOERR_NOMEM = (IOERR | (12 << 8)),
        IOERR_ACCESS = (IOERR | (13 << 8)),
        IOERR_CHECKRESERVEDLOCK = (IOERR | (14 << 8)),
        IOERR_LOCK = (IOERR | (15 << 8)),
        IOERR_CLOSE = (IOERR | (16 << 8)),
        IOERR_DIR_CLOSE = (IOERR | (17 << 8)),
        IOERR_SHMOPEN = (IOERR | (18 << 8)),
        IOERR_SHMSIZE = (IOERR | (19 << 8)),
        IOERR_SHMLOCK = (IOERR | (20 << 8)),
        IOERR_SHMMAP = (IOERR | (21 << 8)),
        IOERR_SEEK = (IOERR | (22 << 8)),
        IOERR_DELETE_NOENT = (IOERR | (23 << 8)),
        IOERR_MMAP = (IOERR | (24 << 8)),
        IOERR_GETTEMPPATH = (IOERR | (25 << 8)),
        IOERR_CONVPATH = (IOERR | (26 << 8)),
        IOERR_VNODE = (IOERR | (27 << 8)),
        IOERR_AUTH = (IOERR | (28 << 8)),
        IOERR_BEGIN_ATOMIC = (IOERR | (29 << 8)),
        IOERR_COMMIT_ATOMIC = (IOERR | (30 << 8)),
        IOERR_ROLLBACK_ATOMIC = (IOERR | (31 << 8)),
        IOERR_DATA = (IOERR | (32 << 8)),
        LOCKED_SHAREDCACHE = (LOCKED | (1 << 8)),
        LOCKED_VTAB = (LOCKED | (2 << 8)),
        BUSY_RECOVERY = (BUSY | (1 << 8)),
        BUSY_SNAPSHOT = (BUSY | (2 << 8)),
        BUSY_TIMEOUT = (BUSY | (3 << 8)),
        CANTOPEN_NOTEMPDIR = (CANTOPEN | (1 << 8)),
        CANTOPEN_ISDIR = (CANTOPEN | (2 << 8)),
        CANTOPEN_FULLPATH = (CANTOPEN | (3 << 8)),
        CANTOPEN_CONVPATH = (CANTOPEN | (4 << 8)),
        CANTOPEN_DIRTYWAL = (CANTOPEN | (5 << 8)) /* Not Used */,
        CANTOPEN_SYMLINK = (CANTOPEN | (6 << 8)),
        CORRUPT_VTAB = (CORRUPT | (1 << 8)),
        CORRUPT_SEQUENCE = (CORRUPT | (2 << 8)),
        CORRUPT_INDEX = (CORRUPT | (3 << 8)),
        READONLY_RECOVERY = (READONLY | (1 << 8)),
        READONLY_CANTLOCK = (READONLY | (2 << 8)),
        READONLY_ROLLBACK = (READONLY | (3 << 8)),
        READONLY_DBMOVED = (READONLY | (4 << 8)),
        READONLY_CANTINIT = (READONLY | (5 << 8)),
        READONLY_DIRECTORY = (READONLY | (6 << 8)),
        ABORT_ROLLBACK = (ABORT | (2 << 8)),
        CONSTRAINT_CHECK = (CONSTRAINT | (1 << 8)),
        CONSTRAINT_COMMITHOOK = (CONSTRAINT | (2 << 8)),
        CONSTRAINT_FOREIGNKEY = (CONSTRAINT | (3 << 8)),
        CONSTRAINT_FUNCTION = (CONSTRAINT | (4 << 8)),
        CONSTRAINT_NOTNULL = (CONSTRAINT | (5 << 8)),
        CONSTRAINT_PRIMARYKEY = (CONSTRAINT | (6 << 8)),
        CONSTRAINT_TRIGGER = (CONSTRAINT | (7 << 8)),
        CONSTRAINT_UNIQUE = (CONSTRAINT | (8 << 8)),
        CONSTRAINT_VTAB = (CONSTRAINT | (9 << 8)),
        CONSTRAINT_ROWID = (CONSTRAINT | (10 << 8)),
        CONSTRAINT_PINNED = (CONSTRAINT | (11 << 8)),
        NOTICE_RECOVER_WAL = (NOTICE | (1 << 8)),
        NOTICE_RECOVER_ROLLBACK = (NOTICE | (2 << 8)),
        WARNING_AUTOINDEX = (WARNING | (1 << 8)),
        AUTH_USER = (AUTH | (1 << 8)),
        OK_LOAD_PERMANENTLY = (OK | (1 << 8)),
        OK_SYMLINK = (OK | (2 << 8)),
    }
}
