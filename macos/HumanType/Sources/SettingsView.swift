import SwiftUI

struct SettingsView: View {
    @EnvironmentObject private var appModel: AppModel

    var body: some View {
        Form {
            Section("Typing Speed") {
                Stepper("Minimum WPM: \(appModel.minWPM)", value: $appModel.minWPM, in: 10...260)
                Stepper("Maximum WPM: \(appModel.maxWPM)", value: $appModel.maxWPM, in: 10...260)
            }

            Section("Typing Behavior") {
                VStack(alignment: .leading, spacing: 8) {
                    Text("Typo Rate: \(appModel.typoRate, format: .number.precision(.fractionLength(2)))")
                    Slider(value: $appModel.typoRate, in: 0...0.30, step: 0.01)
                }

                Text("Final typed output must remain identical to the source text. Humanization is simulated through temporary mistakes and delayed corrections.")
                    .font(.footnote)
                    .foregroundStyle(.secondary)
            }

            Section("Updates") {
                Text("Status: \(appModel.updateStatus)")
                    .foregroundStyle(.secondary)

                Button("Check for Updates Now") {
                    appModel.checkForUpdates()
                }
            }

            Section("Branding") {
                Text("App name: HumanType")
                Text("App icon: user-provided keyboard logo")
                    .foregroundStyle(.secondary)
            }
        }
        .formStyle(.grouped)
        .padding(20)
        .frame(width: 520)
    }
}
