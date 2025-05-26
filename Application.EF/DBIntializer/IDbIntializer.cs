using Newtonsoft.Json.Bson;


namespace TBL.EF.DBIntializer
{
    public interface IDbIntializer
    {
        Task Intialize();
    }
}
