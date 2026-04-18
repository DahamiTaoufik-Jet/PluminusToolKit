using System.Collections.Generic;
using UnityEngine;

namespace Pluminus.Data
{
    /// <summary>
    /// Asset de sauvegarde pour les statistiques de performance de l'IA.
    /// Permet de garder l'historique des courbes même après avoir arrêté le jeu.
    /// </summary>
    [CreateAssetMenu(fileName = "NewAnalyticsData", menuName = "Pluminus/Analytics Data")]
    public class PluminusAnalyticsData : ScriptableObject
    {
        public List<float> episodeRewards = new List<float>();
        public List<float> continuousHistory = new List<float>();
        
        public int totalEpisodes = 0;
        public float bestEpisodeReward = float.MinValue;

        public void Clear()
        {
            episodeRewards.Clear();
            continuousHistory.Clear();
            totalEpisodes = 0;
            bestEpisodeReward = float.MinValue;
        }

        public void AddEpisode(float reward)
        {
            episodeRewards.Add(reward);
            if (episodeRewards.Count > 100) episodeRewards.RemoveAt(0);
            
            totalEpisodes++;
            if (reward > bestEpisodeReward) bestEpisodeReward = reward;
        }

        public void AddContinuousPoint(float totalReward)
        {
            continuousHistory.Add(totalReward);
            if (continuousHistory.Count > 300) continuousHistory.RemoveAt(0);
        }
    }
}
