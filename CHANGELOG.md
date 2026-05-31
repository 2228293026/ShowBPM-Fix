# Changelog

## 1.4.4-preview.1 (2026-05-31)

### Fixed
- Fixed ArgumentOutOfRangeException in MoveToNextFloorPatch.Postfix when currentSeqID is out of angleData bounds during simulated input

## 1.4.3 (2026-05-31)

### Fixed
- Fixed NullReferenceException in MoveToNextFloorPatch.Postfix

## 1.4.2 (2026-05-23)

### Added
- **Next BPM** — display the BPM of the next upcoming speed change point
- **Customizable order** — each BPM line now has an Order slider (0–4) to freely arrange display order

### Fixed
- Text alignment now correctly anchors the aligned edge instead of shifting with value changes
- Next BPM now traverses floors to find the next actual speed change, not just the immediate next floor
- Same order values use stable sort with insertion order as tiebreaker

## 1.4.1

*Skipped — changes rolled into 1.4.2.*

## 1.4.0

- Initial release with CI/CD pipeline
- Tile BPM, Real BPM, KPS, Real KPS display
- Speed text in editor
- Multi-language support (Korean, English, Chinese)
- Canvas scaling rewrite for non-1080p resolutions
