namespace BaseFramework.Events {
	public delegate void TypedEventHandler<TSender, TResult>(TSender sender, TResult args);
}
