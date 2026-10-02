using System;
using System.Collections.Generic;

namespace PuzzleSpire.Domain.Puzzle
{
    public sealed class ObstacleImpactSystem : IObstacleImpactSystem
    {
        private static readonly GridPosition[] AdjacentDirections =
        {
            new GridPosition(0, 1),
            new GridPosition(0, -1),
            new GridPosition(-1, 0),
            new GridPosition(1, 0)
        };

        public IReadOnlyList<ObstacleStateChange> ApplyImpacts(
            Board board,
            IReadOnlyList<ObstacleImpactEvent> directImpactEvents,
            IReadOnlyList<GridPosition> removedBlockPositions)
        {
            if (board == null)
            {
                throw new ArgumentNullException(nameof(board));
            }

            if (directImpactEvents == null)
            {
                throw new ArgumentNullException(nameof(directImpactEvents));
            }

            if (removedBlockPositions == null)
            {
                throw new ArgumentNullException(nameof(removedBlockPositions));
            }

            var accumulators = new Dictionary<ObstacleKey, ImpactAccumulator>();
            var orderedKeys = new List<ObstacleKey>();
            var eventKeys = new List<ObstacleKey>();
            var eventKeySet = new HashSet<ObstacleKey>();

            CollectDirectImpacts(
                board,
                directImpactEvents,
                accumulators,
                orderedKeys,
                eventKeys,
                eventKeySet);

            CollectSameCellBlockRemovedImpacts(
                board,
                removedBlockPositions,
                accumulators,
                orderedKeys,
                eventKeys,
                eventKeySet);

            CollectAdjacentImpacts(
                board,
                removedBlockPositions,
                accumulators,
                orderedKeys,
                eventKeys,
                eventKeySet);

            var changes = new List<ObstacleStateChange>(orderedKeys.Count);

            for (int i = 0; i < orderedKeys.Count; i++)
            {
                ObstacleKey key = orderedKeys[i];

                if (!accumulators.TryGetValue(key, out ImpactAccumulator accumulator))
                {
                    continue;
                }

                ICellObstacle obstacle = board.GetObstacle(key.Position, key.Layer);

                if (!(obstacle is IDamageableObstacle damageableObstacle))
                {
                    continue;
                }

                ObstacleImpactKind acceptedKinds =
                    accumulator.Kinds & damageableObstacle.AcceptedImpacts;

                if (acceptedKinds == ObstacleImpactKind.None)
                {
                    continue;
                }

                int damage = CalculateDamage(accumulator);

                if (damage <= 0)
                {
                    continue;
                }

                ObstacleDamageResult damageResult =
                    damageableObstacle.TakeDamage(damage);

                if (!damageResult.WasDamaged)
                {
                    continue;
                }

                changes.Add(
                    new ObstacleStateChange(
                        key.Position,
                        damageableObstacle.DefinitionId,
                        damageableObstacle.Type,
                        damageableObstacle.Layer,
                        acceptedKinds,
                        damageResult));

                if (damageableObstacle.IsDestroyed)
                {
                    board.RemoveObstacle(key.Position, key.Layer);
                }
            }

            return changes.ToArray();
        }

        private static int CalculateDamage(ImpactAccumulator accumulator)
        {
            if (accumulator.DirectDamage > 0)
            {
                return accumulator.DirectDamage;
            }

            return accumulator.HasSameCellBlockRemovedImpact ||
                   accumulator.HasAdjacentImpact
                ? 1
                : 0;
        }

        private static void CollectDirectImpacts(
            Board board,
            IReadOnlyList<ObstacleImpactEvent> impactEvents,
            Dictionary<ObstacleKey, ImpactAccumulator> accumulators,
            List<ObstacleKey> orderedKeys,
            List<ObstacleKey> eventKeys,
            HashSet<ObstacleKey> eventKeySet)
        {
            for (int eventIndex = 0; eventIndex < impactEvents.Count; eventIndex++)
            {
                ObstacleImpactEvent impactEvent = impactEvents[eventIndex];

                if (impactEvent == null)
                {
                    throw new InvalidOperationException(
                        $"Obstacle impact event {eventIndex} is null.");
                }

                eventKeys.Clear();
                eventKeySet.Clear();

                CollectObstaclesAtPositions(
                    board,
                    impactEvent.Positions,
                    eventKeys,
                    eventKeySet);

                for (int keyIndex = 0; keyIndex < eventKeys.Count; keyIndex++)
                {
                    ObstacleKey key = eventKeys[keyIndex];
                    ICellObstacle obstacle = board.GetObstacle(key.Position, key.Layer);

                    if (!(obstacle is IDamageableObstacle damageableObstacle))
                    {
                        continue;
                    }

                    ObstacleImpactKind acceptedKind =
                        impactEvent.Kind & damageableObstacle.AcceptedImpacts;

                    if (acceptedKind == ObstacleImpactKind.None)
                    {
                        continue;
                    }

                    ImpactAccumulator accumulator = GetOrCreateAccumulator(
                        key,
                        accumulators,
                        orderedKeys);

                    accumulator.DirectDamage++;
                    accumulator.Kinds |= acceptedKind;
                }
            }
        }

        private static void CollectSameCellBlockRemovedImpacts(
            Board board,
            IReadOnlyList<GridPosition> removedBlockPositions,
            Dictionary<ObstacleKey, ImpactAccumulator> accumulators,
            List<ObstacleKey> orderedKeys,
            List<ObstacleKey> eventKeys,
            HashSet<ObstacleKey> eventKeySet)
        {
            eventKeys.Clear();
            eventKeySet.Clear();

            CollectObstaclesAtPositions(
                board,
                removedBlockPositions,
                eventKeys,
                eventKeySet);

            for (int i = 0; i < eventKeys.Count; i++)
            {
                ObstacleKey key = eventKeys[i];
                ICellObstacle obstacle = board.GetObstacle(key.Position, key.Layer);

                if (!(obstacle is IDamageableObstacle damageableObstacle))
                {
                    continue;
                }

                if ((damageableObstacle.AcceptedImpacts &
                     ObstacleImpactKind.SameCellBlockRemoved) == 0)
                {
                    continue;
                }

                ImpactAccumulator accumulator = GetOrCreateAccumulator(
                    key,
                    accumulators,
                    orderedKeys);

                accumulator.HasSameCellBlockRemovedImpact = true;
                accumulator.Kinds |= ObstacleImpactKind.SameCellBlockRemoved;
            }
        }

        private static void CollectAdjacentImpacts(
            Board board,
            IReadOnlyList<GridPosition> removedBlockPositions,
            Dictionary<ObstacleKey, ImpactAccumulator> accumulators,
            List<ObstacleKey> orderedKeys,
            List<ObstacleKey> eventKeys,
            HashSet<ObstacleKey> eventKeySet)
        {
            eventKeys.Clear();
            eventKeySet.Clear();

            for (int i = 0; i < removedBlockPositions.Count; i++)
            {
                GridPosition removedPosition = removedBlockPositions[i];

                for (int directionIndex = 0;
                     directionIndex < AdjacentDirections.Length;
                     directionIndex++)
                {
                    GridPosition direction = AdjacentDirections[directionIndex];
                    GridPosition adjacentPosition = new GridPosition(
                        removedPosition.X + direction.X,
                        removedPosition.Y + direction.Y);

                    CollectObstaclesAtPosition(
                        board,
                        adjacentPosition,
                        eventKeys,
                        eventKeySet);
                }
            }

            for (int i = 0; i < eventKeys.Count; i++)
            {
                ObstacleKey key = eventKeys[i];
                ICellObstacle obstacle = board.GetObstacle(key.Position, key.Layer);

                if (!(obstacle is IDamageableObstacle damageableObstacle))
                {
                    continue;
                }

                if ((damageableObstacle.AcceptedImpacts &
                     ObstacleImpactKind.Adjacent) == 0)
                {
                    continue;
                }

                ImpactAccumulator accumulator = GetOrCreateAccumulator(
                    key,
                    accumulators,
                    orderedKeys);

                accumulator.HasAdjacentImpact = true;
                accumulator.Kinds |= ObstacleImpactKind.Adjacent;
            }
        }

        private static void CollectObstaclesAtPositions(
            Board board,
            IReadOnlyList<GridPosition> positions,
            List<ObstacleKey> orderedKeys,
            HashSet<ObstacleKey> keySet)
        {
            for (int i = 0; i < positions.Count; i++)
            {
                CollectObstaclesAtPosition(
                    board,
                    positions[i],
                    orderedKeys,
                    keySet);
            }
        }

        private static void CollectObstaclesAtPosition(
            Board board,
            GridPosition position,
            List<ObstacleKey> orderedKeys,
            HashSet<ObstacleKey> keySet)
        {
            if (!board.IsPlayable(position))
            {
                return;
            }

            for (int layerIndex = 0;
                 layerIndex < (int)ObstacleLayer.Count;
                 layerIndex++)
            {
                ObstacleLayer layer = (ObstacleLayer)layerIndex;
                ICellObstacle obstacle = board.GetObstacle(position, layer);

                if (!(obstacle is IDamageableObstacle))
                {
                    continue;
                }

                var key = new ObstacleKey(position, layer);

                if (keySet.Add(key))
                {
                    orderedKeys.Add(key);
                }
            }
        }

        private static ImpactAccumulator GetOrCreateAccumulator(
            ObstacleKey key,
            Dictionary<ObstacleKey, ImpactAccumulator> accumulators,
            List<ObstacleKey> orderedKeys)
        {
            if (accumulators.TryGetValue(key, out ImpactAccumulator accumulator))
            {
                return accumulator;
            }

            accumulator = new ImpactAccumulator();
            accumulators.Add(key, accumulator);
            orderedKeys.Add(key);
            return accumulator;
        }

        private sealed class ImpactAccumulator
        {
            public int DirectDamage;
            public bool HasAdjacentImpact;
            public bool HasSameCellBlockRemovedImpact;
            public ObstacleImpactKind Kinds;
        }
    }
}