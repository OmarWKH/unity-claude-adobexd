namespace Nostrivia {
  /// <summary>
  /// Full-screen views. Home and Gameplay are mutually exclusive — exactly one
  /// is the active screen inside the current <see cref="ScreenManager"/> Stage.
  /// </summary>
  public enum ScreenId {
    Home,
    Gameplay
  }

  /// <summary>
  /// Overlays that appear on top of the current screen, each with its own
  /// full-screen backdrop. Overlays live and animate inside the current Stage,
  /// so they travel together with the screen they were opened over.
  /// </summary>
  public enum OverlayId {
    Settings,
    Results,
    Leave
  }

  /// <summary>How <see cref="ScreenManager.TransitionToScreen"/> animates between screens.</summary>
  public enum TransitionKind {
    /// <summary>
    /// The incoming screen slides up from below (anchoredPosition.y: -H to 0)
    /// within the current Stage. The outgoing screen (and any of its overlays)
    /// is covered by the incoming screen and retired once the slide completes.
    /// </summary>
    SlideUp,

    /// <summary>
    /// A brand new Stage is created above the viewport (anchoredPosition.y = +H)
    /// holding the incoming screen. The old Stage (current screen + any overlays
    /// on top of it) and the new Stage then animate simultaneously — old Stage
    /// down to -H, new Stage up to 0 — so the outgoing screen and its overlays
    /// slide away together while the incoming screen drops into place. The old
    /// Stage container is torn down once its slide-out completes.
    /// </summary>
    DualSlide
  }
}
