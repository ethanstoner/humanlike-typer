import Foundation

final class UpdateChecker {
    private let releasesURL = URL(string: "https://api.github.com/repos/ethanstoner/humanlike-typer/releases/latest")!

    func checkForUpdates(completion: @escaping (String) -> Void) {
        let task = URLSession.shared.dataTask(with: releasesURL) { data, _, error in
            if let error {
                completion("Update check failed: \(error.localizedDescription)")
                return
            }

            guard let data else {
                completion("Update check failed: empty response")
                return
            }

            do {
                let release = try JSONDecoder().decode(GitHubRelease.self, from: data)
                completion("Latest release: \(release.tagName)")
            } catch {
                completion("Update check failed: invalid response")
            }
        }

        task.resume()
    }
}

private struct GitHubRelease: Decodable {
    let tagName: String

    enum CodingKeys: String, CodingKey {
        case tagName = "tag_name"
    }
}
