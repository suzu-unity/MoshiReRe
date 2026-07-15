# Random manual eyecatch

Use the Naninovel command at a chapter break or another major transition:

```nani
@eyecatch
@back NextSceneBackground
```

The default command fades to `title&eyecatch/kuro` after three seconds. The next
scene should therefore set its background immediately after the command.

Optional parameters:

```nani
@eyecatch hold:4 fade:0.6
@eyecatch images:title&eyecatch/Load01,title&eyecatch/Load02
@eyecatch end:none hideUI:false
```

Edit `Assets/Resources/MoshiReRe/eyecatch_pool.txt` to change the shared image
pool. Each entry must also be registered as a Naninovel background appearance.
The pool is shuffled and exhausted before it is refilled, and the same image is
not shown twice in a row when two or more images are available.
