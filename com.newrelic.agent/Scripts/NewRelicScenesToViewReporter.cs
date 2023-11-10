using UnityEngine.SceneManagement;

namespace NewRelic
{

    internal class NewRelicScenesToViewReporter : System.IDisposable
    {
        private string activeSceneName;
        private string interactionId;
        public NewRelicScenesToViewReporter()
        {
            SceneManager.activeSceneChanged += NewRelicActiveSceneChangedHandler;
        }

        public void StartViewFromScene(Scene scene)
        {
            activeSceneName = scene.name;
            interactionId = NewRelicAgent.StartInteractionWithName(activeSceneName);

        }

        public void EndViewFromScene(Scene scene)
        {
            NewRelicAgent.StopCurrentInteraction(interactionId);
        }

        private void NewRelicActiveSceneChangedHandler(Scene current, Scene next)
        {
            if (activeSceneName != null)
            {
                NewRelicAgent.StopCurrentInteraction(interactionId);
            }

            // It is important to note that current is only initialized in the case of additive scenes changing which one is active.
            if (current.name != null && !current.name.Equals(activeSceneName))
            {
                EndViewFromScene(current);
            }

            StartViewFromScene(next);
        }

        public void Dispose()
        {
            SceneManager.activeSceneChanged -= NewRelicActiveSceneChangedHandler;
        }
    }


}