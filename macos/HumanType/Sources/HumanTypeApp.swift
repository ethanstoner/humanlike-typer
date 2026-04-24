import SwiftUI

@main
struct HumanTypeApp: App {
    @StateObject private var appModel = AppModel()

    var body: some Scene {
        MenuBarExtra("HumanType", systemImage: appModel.menuBarSymbolName) {
            MenuBarView()
                .environmentObject(appModel)
        }
        .menuBarExtraStyle(.window)

        Settings {
            SettingsView()
                .environmentObject(appModel)
        }
    }
}
