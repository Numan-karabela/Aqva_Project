namespace Domain.Entity
{
    public abstract class BaseEntity : IBaseEntity
    {
        public long Id { get; set; }
    }

    public interface IBaseEntity
    {
        public long Id { get; set; }
    }
}