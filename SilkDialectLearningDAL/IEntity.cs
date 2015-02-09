using System;

namespace SilkDialectLearning.DAL
{
    public interface IEntity
    {
        Guid Id { get; set; }

        string Name { get; set; }

        string Description { get; set; }
    }
}
