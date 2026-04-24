import SwiftUI

struct MenuBarView: View {
    @EnvironmentObject private var appModel: AppModel

    var body: some View {
        VStack(alignment: .leading, spacing: 12) {
            VStack(alignment: .leading, spacing: 4) {
                Text("HumanType")
                    .font(.headline)
                Text(appModel.statusText)
                    .font(.caption)
                    .foregroundStyle(.secondary)
            }

            Button("Type Clipboard") {
                appModel.typeClipboard()
            }
            .disabled(appModel.isTyping)

            Button("Stop Typing") {
                appModel.stopTyping()
            }
            .disabled(!appModel.isTyping)

            Divider()

            Button("Check for Updates") {
                appModel.checkForUpdates()
            }

            Button("Settings…") {
                NSApp.sendAction(Selector(("showSettingsWindow:")), to: nil, from: nil)
            }

            Divider()

            Button("Quit HumanType") {
                NSApplication.shared.terminate(nil)
            }
        }
        .padding(14)
        .frame(width: 260)
    }
}
