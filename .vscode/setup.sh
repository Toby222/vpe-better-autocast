set -e

script_dir=$(dirname $(readlink -f $0))
mod_dir=$(dirname $script_dir)
pushd $script_dir

mod_name="${mod_dir##*/}"

# Try to create symlink in RimWorld mod directory
ln -s $mod_dir -t "$(readlink -f ~/.steam/steam/steamapps/common/RimWorld/Mods)" -v || echo "Target directory already exists. This is expected on re-runs"

popd