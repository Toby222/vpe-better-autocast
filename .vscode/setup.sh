set -e
set -x

script_dir=$(dirname $(readlink -f $0))
mod_dir=$(dirname $script_dir)
pushd $script_dir

mod_name="${mod_dir##*/}"
target_dir=${1:-$(realpath ~/.local/share/Steam/steamapps/common/RimWorld/Mods/)/$mod_name}

if [ "$target_dir" = "$mod_dir" ]; then
  echo "Target directory is the same as the mod directory. Cloned in RimWorld/Mods. Skipping setup"
else
  mkdir -p $target_dir
  rsync -av --exclude '$mod_dir/*' --delete $mod_dir/1.4 $mod_dir/1.5 $mod_dir/1.6 $mod_dir/Compatibility $mod_dir/LoadFolders.xml $mod_dir/About $mod_dir/README $mod_dir/LICENSE $mod_dir/Changelog.md $target_dir
fi

popd
