namespace Legendary.Core.Types
{
    public class Saves
	{
        /// <summary>
        /// Save vs. spell.
        /// </summary>
        public int Spell { get; set; } = 8;

        /// <summary>
        /// Save vs. negative.
        /// </summary>
        public int Negative { get; set; } = 8;

        /// <summary>
        /// Save vs. maledictive.
        /// </summary>
        public int Maledictive { get; set; } = 8;

        /// <summary>
        /// Save vs. afflictive.
        /// </summary>
        public int Afflictive { get; set; } = 8;

        /// <summary>
        /// Save vs. death.
        /// </summary>
        public int Death { get; set; } = 8;
    }
}

