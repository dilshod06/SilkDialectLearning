using System;

namespace SilkDialectLearningDAL
{
    public interface IEntity
    {
        Guid Id { get; set; }

        string Name { get; set; }

        string Description { get; set; }
    }
}
