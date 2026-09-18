using System;
using System.Collections.Generic;
using FootballLife.Domain;

namespace FootballLife.Simulation
{
    /// <summary>
    /// Encapsulates the outcome of parsing and validating static game world content.
    /// </summary>
    public sealed record WorldDataLoadResult
    {
        public bool IsSuccess => Errors.Count == 0 && WorldState is not null;
        public WorldState? WorldState { get; init; }
        public IReadOnlyList<ValidationError> Errors { get; init; }

        private WorldDataLoadResult(WorldState? worldState, IReadOnlyList<ValidationError> errors)
        {
            WorldState = worldState;
            Errors = errors ?? Array.Empty<ValidationError>();
        }

        public static WorldDataLoadResult Success(WorldState worldState) =>
            new WorldDataLoadResult(worldState ?? throw new ArgumentNullException(nameof(worldState)), Array.Empty<ValidationError>());

        public static WorldDataLoadResult Failure(IReadOnlyList<ValidationError> errors) =>
            new WorldDataLoadResult(null, errors ?? throw new ArgumentNullException(nameof(errors)));
    }
}
