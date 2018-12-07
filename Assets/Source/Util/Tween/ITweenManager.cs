using CreateAR.EnkluPlayer.IUX;

namespace CreateAR.EnkluPlayer.Util
{
    /// <summary>
    /// Describes an object that can create and manage tweens.
    /// </summary>
    public interface ITweenManager
    {
        /// <summary>
        /// Creates a tween for a float value.
        /// </summary>
        /// <param name="element">The element to affect.</param>
        /// <param name="data">The data for the tween.</param>
        /// <returns></returns>
        Tween Float(Element element, TweenData data);

        /// <summary>
        /// Creates a tween for a color value.
        /// </summary>
        /// <param name="element">The element to affect.</param>
        /// <param name="data">The data for the tween.</param>
        /// <returns></returns>
        Tween Col4(Element element, TweenData data);

        /// <summary>
        /// Creates a tween for a vec3 value.
        /// </summary>
        /// <param name="element">The element to affect.</param>
        /// <param name="data">The data for the tween.</param>
        /// <returns></returns>
        Tween Vec3(Element element, TweenData data);

        /// <summary>
        /// Starts a tween.
        /// </summary>
        /// <param name="tween">The tween to start.</param>
        void Start(Tween tween);

        /// <summary>
        /// Stops a tween for good.
        /// </summary>
        /// <param name="tween">The tween to stop.</param>
        void Stop(Tween tween);

        /// <summary>
        /// Stops all tweens.
        /// </summary>
        void StopAll();

        /// <summary>
        /// Pauses a tween.
        /// </summary>
        /// <param name="tween">The tween to pause.</param>
        void Pause(Tween tween);

        /// <summary>
        /// Resumes a tween.
        /// </summary>
        /// <param name="tween">The tween to resume.</param>
        void Resume(Tween tween);

        /// <summary>
        /// Call to advance tweens.
        /// </summary>
        /// <param name="dt">The number of seconds to advance the tweens.</param>
        void Update(float dt);
    }
}